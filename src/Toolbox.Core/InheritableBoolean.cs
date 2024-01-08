using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox
{
    /// <summary>
    /// Calls to make <see cref="bool"/> inheriable values.
    /// </summary>
    public class InheritableBoolean
    {
        public static InheritableBoolean True { get; } = new InheritableBoolean();
        public static InheritableBoolean False { get; } = new InheritableBoolean();
        public static InheritableBoolean Default { get; } = new InheritableBoolean();

        public static implicit operator InheritableBoolean(bool value) => value ? True : False;
        public static explicit operator bool(InheritableBoolean value)
        {
            if (value == null) throw new ArgumentNullException();
            if (value == Default) throw new InvalidCastException("Default can not be converted.");

            return value == True;
        }

        public static bool operator true(InheritableBoolean value) => value == True;
        public static bool operator false(InheritableBoolean value) => value == False;

        public static InheritableBoolean operator &(InheritableBoolean value1, InheritableBoolean value2)
        {
            if (value1 == Default) return value2;
            if (value2 == Default) return value1;

            if (value1==False || value2==False) return False;
            return True;
        }

        public static InheritableBoolean operator |(InheritableBoolean value1, InheritableBoolean value2)
        {
            if (value1 == Default) return value2;
            if (value2 == Default) return value1;

            if (value1 == True || value2 == True) return True;
            return False;
        }

        public InheritableBoolean Inherit(InheritableBoolean value)
        {
            if (this != Default) return this;

            return value;
        }
    }
}
