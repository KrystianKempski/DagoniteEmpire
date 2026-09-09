namespace DA_Common.Barony
{
    /// <summary>
    /// Resource Balance ledger rows that mirror stock changes already applied elsewhere
    /// (Budget transfers, debts). Custom “Add Source” rows apply stocks on the Resources page.
    /// </summary>
    public static class ResourceBalanceLedger
    {
        public const string TransferToPurseName = "Transfer to baron purse";
        public const string TransferFromPurseName = "Transfer from baron purse";

        public static bool IsTransferToPurse(string? name) =>
            string.Equals(name?.Trim(), TransferToPurseName, StringComparison.OrdinalIgnoreCase);

        public static bool IsTransferFromPurse(string? name) =>
            string.Equals(name?.Trim(), TransferFromPurseName, StringComparison.OrdinalIgnoreCase);

        public static bool IsPurseTransfer(string? name) =>
            IsTransferToPurse(name) || IsTransferFromPurse(name);

        /// <summary>
        /// Loan open / early repayment lines — treasury already moved with the debt record.
        /// Edit/delete belongs on Budget → Debts, not the ledger trash icon.
        /// </summary>
        public static bool IsDebtLinked(string? name)
        {
            var n = name?.Trim() ?? "";
            return n.StartsWith("Loan from ", StringComparison.OrdinalIgnoreCase)
                   || n.StartsWith("Loan to ", StringComparison.OrdinalIgnoreCase)
                   || n.StartsWith("Early debt payment to ", StringComparison.OrdinalIgnoreCase)
                   || n.StartsWith("Early loan repayment from ", StringComparison.OrdinalIgnoreCase);
        }
    }
}
