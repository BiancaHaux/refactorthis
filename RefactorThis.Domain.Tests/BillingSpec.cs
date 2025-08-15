using System;
using System.Collections.Generic;
using Xunit;
using BillingSystem.Persistence;

namespace BillingSystem.Domain.Tests
{
    public class BillingSpec
    {
        [Fact]
        public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference( )
        {
            var repo = new BillingRepository( );

            Invoice invoice = null;
            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "non-existent-invoice",
                Amount = 10
            };
            var failureMessage = "";

            try
            {
                var result = paymentProcessor.ProcessPayment( payment );
            }
            catch ( InvalidOperationException e )
            {
                failureMessage = e.Message;
            }

            Assert.Equal("There is no invoice matching this payment", failureMessage);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
        {
            var repo = new BillingRepository( );

            var invoice = new Invoice( repo )
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            repo.Add( invoice );

            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "test-invoice",
                Amount = 5
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal( "no payment needed", result );
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
        {
            var repo = new BillingRepository( );

            var invoice = new Invoice( repo )
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 10,
                        Reference = "existing-payment"
                    }
                }
            };
            repo.Add( invoice );

            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "test-invoice",
                Amount = 5
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal("invoice was already fully paid", result );
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
        {
            var repo = new BillingRepository( );
            var invoice = new Invoice( repo )
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5,
                        Reference = "partial-payment-1"
                    }
                }
            };
            repo.Add( invoice );

            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "test-invoice",
                Amount = 6
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal("the payment is greater than the partial amount remaining", result );
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var repo = new BillingRepository( );
            var invoice = new Invoice( repo )
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>( )
            };
            repo.Add( invoice );

            var paymentProcessor = new BillingService(repo);

            var payment = new Payment()
            {
                Reference = "test-invoice",
                Amount = 6
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal("the payment is greater than the invoice amount", result );
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var repo = new BillingRepository( );
            var invoice = new Invoice( repo )
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5,
                        Reference = "partial-payment-1"
                    }
                }
            };
            repo.Add( invoice );

            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "test-invoice",
                Amount = 5
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal("final partial payment received, invoice is now fully paid", result );
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
        {
            var repo = new BillingRepository( );
            var invoice = new Invoice( repo )
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>( )
                {
                    new Payment( )
                    {
                        Amount = 10,
                        Reference = "existing-full-payment"
                    }
                }
            };
            repo.Add( invoice );

            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "test-invoice",
                Amount = 10
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal( "invoice was already fully paid", result );
        }

        [Fact]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
        {
            var repo = new BillingRepository( );
            var invoice = new Invoice( repo )
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5,
                        Reference = "partial-payment-1"
                    }
                }
            };
            repo.Add( invoice );

            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "test-invoice",
                Amount = 1
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal( "another partial payment received, still not fully paid", result );
        }

        [Fact]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
        {
            var repo = new BillingRepository( );
            var invoice = new Invoice( repo )
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>( )
            };
            repo.Add( invoice );

            var paymentProcessor = new BillingService( repo );

            var payment = new Payment( )
            {
                Reference = "test-invoice",
                Amount = 1
            };

            var result = paymentProcessor.ProcessPayment( payment );

            Assert.Equal( "invoice is now partially paid", result );
        }
    }
}