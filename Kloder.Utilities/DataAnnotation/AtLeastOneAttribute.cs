using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

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
