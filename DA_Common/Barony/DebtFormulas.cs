namespace DA_Common.Barony
{
    /// <summary>Loan / debt interest and turn payment rules.</summary>
    public static class DebtFormulas
    {
        public const decimal MinPrincipal = 1m;
        public const decimal MaxInterestPercentPerTurn = 20m;

        public static decimal ClampInterestPercent(decimal value) =>
            PpbFormat.Round(Math.Clamp(value, 0m, MaxInterestPercentPerTurn));

        public static decimal AccrueInterest(decimal principalRemaining, decimal interestRatePercentPerTurn)
        {
            if (principalRemaining <= 0m || interestRatePercentPerTurn <= 0m)
                return PpbFormat.Round(Math.Max(0m, principalRemaining));

            return PpbFormat.Round(principalRemaining + principalRemaining * (interestRatePercentPerTurn / 100m));
        }

        /// <summary>Scheduled payment when the barony owes (Taken). Pays up to treasury balance (strict option A).</summary>
        public static decimal ComputeTakenPayment(decimal paymentPerTurn, decimal principalRemaining, decimal treasuryGold)
        {
            if (principalRemaining <= 0m)
                return 0m;

            var scheduled = paymentPerTurn <= 0m
                ? 0m
                : Math.Min(paymentPerTurn, principalRemaining);

            return PpbFormat.Round(Math.Min(scheduled, Math.Max(0m, treasuryGold)));
        }

        /// <summary>Scheduled payment when the barony is owed (Given). Counterparty always pays in full up to remaining.</summary>
        public static decimal ComputeGivenPayment(decimal paymentPerTurn, decimal principalRemaining)
        {
            if (principalRemaining <= 0m)
                return 0m;

            if (paymentPerTurn <= 0m)
                return 0m;

            return PpbFormat.Round(Math.Min(paymentPerTurn, principalRemaining));
        }

        public sealed record TurnPaymentSummary(
            decimal TakenPayments,
            decimal GivenReceipts,
            decimal TakenAfterInterest,
            decimal GivenAfterInterest);

        /// <summary>Project debt cash flow for one turn (after interest accrual).</summary>
        public static TurnPaymentSummary ProjectTurnPayments(
            IEnumerable<(string Direction, decimal PrincipalRemaining, decimal InterestRatePercent, decimal PaymentPerTurn)> debts,
            decimal treasuryGold)
        {
            decimal takenAfterInterest = 0m;
            decimal givenAfterInterest = 0m;
            decimal takenPayments = 0m;
            decimal givenReceipts = 0m;
            var remainingTreasury = treasuryGold;

            foreach (var (direction, remaining, rate, paymentPerTurn) in debts)
            {
                if (!DebtDirection.IsActive(direction) || remaining <= 0m)
                    continue;

                var afterInterest = AccrueInterest(remaining, rate);
                if (DebtDirection.IsTaken(direction))
                {
                    takenAfterInterest += afterInterest;
                    var pay = ComputeTakenPayment(paymentPerTurn, afterInterest, remainingTreasury);
                    takenPayments += pay;
                    remainingTreasury = PpbFormat.Round(Math.Max(0m, remainingTreasury - pay));
                }
                else if (DebtDirection.IsGiven(direction))
                {
                    givenAfterInterest += afterInterest;
                    givenReceipts += ComputeGivenPayment(paymentPerTurn, afterInterest);
                }
            }

            return new TurnPaymentSummary(
                takenPayments,
                givenReceipts,
                PpbFormat.Round(takenAfterInterest),
                PpbFormat.Round(givenAfterInterest));
        }

        /// <summary>Rough estimate of turns until paid off at fixed payment (ignores interest growth).</summary>
        public static int? EstimateTurnsToPayOff(decimal principalRemaining, decimal paymentPerTurn, decimal interestRatePercentPerTurn)
        {
            if (principalRemaining <= 0m)
                return 0;
            if (paymentPerTurn <= 0m && interestRatePercentPerTurn > 0m)
                return null;

            var balance = principalRemaining;
            for (var turn = 1; turn <= 500; turn++)
            {
                balance = AccrueInterest(balance, interestRatePercentPerTurn);
                balance = PpbFormat.Round(balance - ComputeGivenPayment(paymentPerTurn, balance));
                if (balance <= 0m)
                    return turn;
            }

            return null;
        }
    }

    /// <summary>Whether the barony borrowed (Taken) or lent (Given).</summary>
    public readonly struct DebtDirection
    {
        public const string Taken = "Taken";
        public const string Given = "Given";

        public static bool IsTaken(string? direction) =>
            string.Equals(direction?.Trim(), Taken, StringComparison.OrdinalIgnoreCase);

        public static bool IsGiven(string? direction) =>
            string.Equals(direction?.Trim(), Given, StringComparison.OrdinalIgnoreCase);

        public static bool IsActive(string? direction) => IsTaken(direction) || IsGiven(direction);

        public static string Normalize(string? direction) =>
            IsGiven(direction) ? Given : Taken;
    }
}
