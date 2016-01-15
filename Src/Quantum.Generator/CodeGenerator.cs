using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Quantum.Generator
{
    public class CodeGenerator : ICodeGenerator
    {
        private readonly CodeCompileUnit _targetUnit;        
        private readonly string _path;
        private readonly string _namespace;
        private readonly string _fileName;

        public CodeGenerator(string @namespace, string path, string fileName)
        {
            _path = path;
            _fileName = fileName;
            _targetUnit = new CodeCompileUnit();
            _namespace = @namespace;            
        }

        public void GenerateClasses(IList<DatabaseTableInfo> tables)
        {
            CodeNamespace code = new CodeNamespace(_namespace);
            code.Imports.Add(new CodeNamespaceImport("System"));

            foreach (DatabaseTableInfo table in tables)
            {
                var targetClass = new CodeTypeDeclaration(table.ClassName)
                {
                    IsClass = true,
                    TypeAttributes = TypeAttributes.Public
                };
                
                GenerateFieldsAndProperties(table.Fields, targetClass);
                                
                code.Types.Add(targetClass);                
            }

            _targetUnit.Namespaces.Add(code);

            GenerateCSharpCode();
        }

        private void GenerateFieldsAndProperties(IEnumerable<DatabaseFieldInfo> fields, CodeTypeDeclaration targetClass)
        {
            foreach (DatabaseFieldInfo field in fields)
            {
                AddFields(field.FieldName, field.Name, targetClass);
                AddProperties(field.AttributeName, field.FieldName, targetClass);
            }
        }

        private void AddFields(string name, string sqlName, CodeTypeDeclaration targetClass)
        {            
            CodeMemberField field = new CodeMemberField
            {
                Attributes = MemberAttributes.Private,
                Name = name,
                Type = new CodeTypeReference(typeof(string)),
                InitExpression = new CodePrimitiveExpression(sqlName)
            };

            targetClass.Members.Add(field);
        }

        private void AddProperties(string name, string filedName, CodeTypeDeclaration targetClass)
        {
            CodeMemberProperty property = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = name,
                HasGet = true,
                HasSet = false,
                Type = new CodeTypeReference(typeof(string)),
            };
            
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), filedName)));            

            targetClass.Members.Add(property);
        }        
        

        private void GenerateCSharpCode()
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions { BracingStyle = "C" };

            using (StreamWriter sourceWriter = new StreamWriter(Path.Combine(_path, _fileName + ".cs")))
            {
                provider.GenerateCodeFromCompileUnit(_targetUnit, sourceWriter, options);
            }
        }
    }
}
