﻿using System.Diagnostics;
using Macad.Core.Topology;
using Macad.Common.Serialization;
using Macad.Occt;

namespace Macad.Core.Shapes
{
    [SerializeType]
    public sealed class BooleanFuse : BooleanBase
    {
        public BooleanFuse()
        {
            Name = "Boolean Fuse";
        }

        //--------------------------------------------------------------------------------------------------

        protected override BRepAlgoAPI_BooleanOperation CreateAlgoApi()
        {
            return new BRepAlgoAPI_Fuse();
        }

        //--------------------------------------------------------------------------------------------------

        public static BooleanFuse Create(Body targetBody, IShapeOperand operand)
        {
            Debug.Assert(targetBody != null);

            var boolean = new BooleanFuse();
            targetBody.AddShape(boolean);
            boolean.AddOperand(operand);

            return boolean;
        }

        //--------------------------------------------------------------------------------------------------

        public static BooleanFuse Create(Body targetBody, IShapeOperand[] operands)
        {
            Debug.Assert(targetBody != null);

            var boolean = new BooleanFuse();
            targetBody.AddShape(boolean);
            foreach (var shapeOperand in operands)
            {
                boolean.AddOperand(shapeOperand);
            }

            return boolean;
        }

    }
}