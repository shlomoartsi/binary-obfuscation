using ObfuscationTransform.Core.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Parser.Factory
{
    public interface IBasicBlockParserFactory : IFactory<IBasicBlockParser> { };
    public interface IBasicBlockEpilogParserFactory : IFactory<IBasicBlockEpilogParser> { };
    public interface ICodeParserFactory : IFactory<ICodeParser> { };
    public interface IFunctionEpilogParserFactory : IFactory<IFunctionEpilogParser> { };
    public interface IFunctionParserFactory : IFactory<IFunctionParser> { }
    public interface IFunctionPrologParserFactory : IFactory<IFunctionPrologParser> { };

    public class BasicBlockParserFactory : Factory<IBasicBlockParser> { };
    public class BasicBlockEpliogParserFactory : Factory<IBasicBlockEpilogParserFactory> {};
    public class CodeParserFactory : Factory<ICodeParser> { };
    public class FunctionEpilogParserFactory : Factory<IFunctionEpilogParser> { };
    public class FunctionParserFactory : Factory<IFunctionParser> { }
    public class FunctionPrologParserFactory : Factory<IFunctionPrologParser> { };

}
