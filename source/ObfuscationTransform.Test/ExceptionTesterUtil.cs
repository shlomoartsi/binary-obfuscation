using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Test
{
    static class ExceptionTesterUtil
    {
        public static bool CheckIfExceptionIsThrown<T>(Action action, ref Exception ex) where T : Exception
        {
            try
            {
                action();
            }
            catch (T ex1)
            {
                ex = ex1;
                return true;
            }
            catch (Exception )
            {
                
            }

            return false;
        }
    }
}
