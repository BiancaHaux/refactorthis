using BillingSystem.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BillingSystem.Domain
{
	public class BillingService
	{
		private readonly BillingRepository _billingRepository;

		public BillingService(BillingRepository BillingRepository )
		{
			_billingRepository = BillingRepository;
		}

		public string ProcessPayment( Payment payment )
		{
            var invoice = _billingRepository.GetInvoice(payment.Reference);

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            var result = ProcessPaymentForInvoice(invoice, payment);
            invoice.Save();
            return result;
        }

        private string ProcessPaymentForInvoice(Invoice invoice, Payment payment)
        {
            // Handle zero-amount invoices
            if (invoice.Amount == 0)
            {
                if (invoice.Payments?.Any() == true)
                {
                    throw new InvalidOperationException(PaymentMessages.InvalidState);
                }
                return PaymentMessages.NoPaymentNeeded;
            }

            // Check if already fully paid
            if (IsInvoiceFullyPaid(invoice))
            {
                return PaymentMessages.AlreadyFullyPaid;
            }

            // Validate payment amount
            if (IsPaymentTooLarge(invoice, payment))
            {
                return invoice.Payments?.Any() == true
                    ? PaymentMessages.PaymentTooLarge
                    : PaymentMessages.PaymentExceedsInvoice;
            }

            // Process the payment
            ApplyPaymentToInvoice(invoice, payment);

            // Determine result message
            return DeterminePaymentResultMessage(invoice, payment);
        }

        private string DeterminePaymentResultMessage(Invoice invoice, Payment payment)
        {
            var remainingAmount = GetRemainingAmount(invoice);
            var wasFirstPayment = invoice.Payments.Count == 1;
            var isNowFullyPaid = remainingAmount == 0;

            if (isNowFullyPaid)
            {
                return wasFirstPayment ? PaymentMessages.FullyPaid : PaymentMessages.FinalPayment;
            }
            else
            {
                return wasFirstPayment ? PaymentMessages.NewPartialPayment : PaymentMessages.PartialPayment;
            }
        }

        private bool IsInvoiceFullyPaid(Invoice invoice)
        {
            return invoice.Payments != null &&
                   invoice.Payments.Any() &&
                   invoice.Payments.Sum(x => x.Amount) == invoice.Amount;
        }

        private decimal GetRemainingAmount(Invoice invoice)
        {
            return invoice.Amount - invoice.AmountPaid;
        }

        private bool IsPaymentTooLarge(Invoice invoice, Payment payment)
        {
            var remaining = GetRemainingAmount(invoice);
            return payment.Amount > remaining;
        }

        private void ApplyPaymentToInvoice(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount += CalculateTax(payment.Amount, invoice.Type);

            if (invoice.Payments == null)
                invoice.Payments = new List<Payment>();

            invoice.Payments.Add(payment);
        }

        private decimal CalculateTax(decimal amount, InvoiceType invoiceType)
        {
            return amount * PaymentConstants.TaxRate;
        }
    }
}