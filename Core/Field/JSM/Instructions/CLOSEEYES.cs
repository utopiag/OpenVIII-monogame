﻿using System;


namespace OpenVIII
{
    internal sealed class CLOSEEYES : JsmInstruction
    {
        public CLOSEEYES()
        {
        }

        public CLOSEEYES(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(CLOSEEYES)}()";
        }
    }
}