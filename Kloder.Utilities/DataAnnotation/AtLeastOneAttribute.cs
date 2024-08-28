using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Kloder.Utilities.DataAnnotation
{
    public class AtLeastOneAttribute : ValidationAttribute
    {
        public Expression MyProperty { get; set; }

        public AtLeastOneAttribute(Expression<Func<Object, Object>> param)
        {
            MyProperty = param;
        }


        public override bool IsValid(object value)
        {
            return true;
        }
    }
}
