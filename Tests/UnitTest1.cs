using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Squish;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var sourceOne = new StringBuilder();
            sourceOne.AppendLine("using System;");
            sourceOne.AppendLine("using System.Collections.Generic;");
            sourceOne.AppendLine("using System.Linq;");
            sourceOne.AppendLine("using System.Text;");
            sourceOne.AppendLine("");
            sourceOne.AppendLine("namespace Alpha.Bravo");
            sourceOne.AppendLine("{");
            sourceOne.AppendLine("    class Apple");
            sourceOne.AppendLine("    {");
            sourceOne.AppendLine("      ");
            sourceOne.AppendLine("        public string Name { get; set; }");
            sourceOne.AppendLine("        // public string SomethingElse { get; set; }");
            sourceOne.AppendLine("        public int Age { get; set; }");
            sourceOne.AppendLine("        public const char Id = 'i';");
            sourceOne.AppendLine("        private const char Potato = 'i';");
            sourceOne.AppendLine("");
            sourceOne.AppendLine("        private Guid guid = System.Guid.NewGuid();");
            sourceOne.AppendLine("        public Guid Uid");
            sourceOne.AppendLine("        {");
            sourceOne.AppendLine("            get");
            sourceOne.AppendLine("            {");
            sourceOne.AppendLine("                return guid;");
            sourceOne.AppendLine("            }");
            sourceOne.AppendLine("        }");
            sourceOne.AppendLine("");
            sourceOne.AppendLine("        public string DoSomething(string bubba)");
            sourceOne.AppendLine("        {");
            sourceOne.AppendLine("            return \"private string DoSomethingElse(int age) { }\";");
            sourceOne.AppendLine("        }");
            sourceOne.AppendLine("");
            sourceOne.AppendLine("        /*");
            sourceOne.AppendLine("         public string DoSomethingAgain(string charlie)");
            sourceOne.AppendLine("        {");
            sourceOne.AppendLine("            return \"private string DoSomethingFurther(int count) { }\";");
            sourceOne.AppendLine("        }");
            sourceOne.AppendLine("         */");
            sourceOne.AppendLine("    }");
            sourceOne.AppendLine("}");

            var sourceTwo = new StringBuilder();
            sourceTwo.AppendLine("using System;");
            sourceTwo.AppendLine("using System.Collections.Generic;");
            sourceTwo.AppendLine("using System.Linq;");
            sourceTwo.AppendLine("using System.Text;");
            sourceTwo.AppendLine("");
            sourceTwo.AppendLine("namespace Charlie.Delta");
            sourceTwo.AppendLine("{");
            sourceTwo.AppendLine("    class Banana");
            sourceTwo.AppendLine("    {");
            sourceTwo.AppendLine("      ");
            sourceTwo.AppendLine("        public string Name { get; set; }");
            sourceTwo.AppendLine("        // public string SomethingElse { get; set; }");
            sourceTwo.AppendLine("        public int Age { get; set; }");
            sourceTwo.AppendLine("        public const char OtherId = 'i';");
            sourceTwo.AppendLine("        private const char Potato = 'i';");
            sourceTwo.AppendLine("");
            sourceTwo.AppendLine("        private Guid guid = System.Guid.NewGuid();");
            sourceTwo.AppendLine("        public Guid Uid");
            sourceTwo.AppendLine("        {");
            sourceTwo.AppendLine("            get");
            sourceTwo.AppendLine("            {");
            sourceTwo.AppendLine("                return guid;");
            sourceTwo.AppendLine("            }");
            sourceTwo.AppendLine("        }");
            sourceTwo.AppendLine("");
            sourceTwo.AppendLine("        public string DoSomething(string bubba)");
            sourceTwo.AppendLine("        {");
            sourceTwo.AppendLine("            return \"private string DoSomethingElse(int age) { }\";");
            sourceTwo.AppendLine("        }");
            sourceTwo.AppendLine("");
            sourceTwo.AppendLine("        /*");
            sourceTwo.AppendLine("         public string DoSomethingAgain(string charlie)");
            sourceTwo.AppendLine("        {");
            sourceTwo.AppendLine("            return \"private string DoSomethingFurther(int count) { }\";");
            sourceTwo.AppendLine("        }");
            sourceTwo.AppendLine("         */");
            sourceTwo.AppendLine("    }");
            sourceTwo.AppendLine("}");

            var squisherClass = Squisher.Emit(sourceOne.ToString(), sourceTwo.ToString());
        }
    }
}
