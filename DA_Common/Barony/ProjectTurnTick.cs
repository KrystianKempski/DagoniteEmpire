namespace DA_Common.Barony
{
    /// <summary>Turn countdown rules for funded barony projects during Resolve.</summary>
    public static class ProjectTurnTick
    {
        public enum Step
        {
            /// <summary>Fully funded, duration &gt; 0 — clock starts next Resolve.</summary>
            WaitAfterFunding,
            /// <summary>Zero-turn project or timer already at 0 — complete now.</summary>
            CompleteNow,
            /// <summary>Decremented the timer; still time left.</summary>
            TickAndContinue,
            /// <summary>Decremented the timer to 0 — complete now.</summary>
            TickAndComplete,
        }

        /// <summary>
        /// Advances the project turn clock after it is fully funded.
        /// </summary>
        public static Step Advance(ref int turnsRemaining, bool justFunded)
        {
            if (justFunded && turnsRemaining > 0)
                return Step.WaitAfterFunding;

            if (turnsRemaining <= 0)
                return Step.CompleteNow;

            turnsRemaining--;
            return turnsRemaining <= 0 ? Step.TickAndComplete : Step.TickAndContinue;
        }
    }
}
