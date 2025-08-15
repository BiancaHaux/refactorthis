namespace BillingSystem.Domain
{
    public static class PaymentConstants
    {
        public const decimal TaxRate = 0.14m;
    }

    public static class PaymentMessages
    {
        public const string NoPaymentNeeded = "no payment needed";
        public const string AlreadyFullyPaid = "invoice was already fully paid";
        public const string PaymentTooLarge = "the payment is greater than the partial amount remaining";
        public const string PaymentExceedsInvoice = "the payment is greater than the invoice amount";
        public const string FinalPayment = "final partial payment received, invoice is now fully paid";
        public const string PartialPayment = "another partial payment received, still not fully paid";
        public const string FullyPaid = "invoice is now fully paid";
        public const string NewPartialPayment = "invoice is now partially paid";
        public const string InvalidState = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
    }
}