using System;

namespace Oan.SoulFrame.Gates
{
    public class FrameLockGate
    {
        public bool CanProceed(SoulFrameSession session, out string reasonCode, out string explanation)
        {
            if (session == null)
            {
                 reasonCode = "SOULFRAME.SESSION_NULL";
                 explanation = "Session is null.";
                 return false;
            }

            if (!session.FrameLock.IsSet)
            {
                reasonCode = "SOULFRAME.FRAMELOCK_REQUIRED";
                explanation = "FrameLock must be set before generation can proceed.";
                return false;
            }

            reasonCode = "SUCCESS";
            explanation = "FrameLock is set.";
            return true;
        }
    }
}
