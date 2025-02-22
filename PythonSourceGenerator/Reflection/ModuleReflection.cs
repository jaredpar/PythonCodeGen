﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PythonSourceGenerator.Reflection
{
    public static class ModuleReflection
    {
        public static List<MethodDeclarationSyntax> MethodsFromModule(PyObject moduleObject, PyModule scope)
        {
            var methods = new List<MethodDeclarationSyntax>();
            // Get methods
            var moduleDir = moduleObject.Dir();
            var callables = new List<string>();
            foreach (PyObject attrName in moduleDir)
            {
                var attr = moduleObject.GetAttr(attrName);
                if (attr.IsCallable() && attr.HasAttr("__name__"))
                {
                    scope.Import("inspect");
                    var moduleName = moduleObject.GetAttr("__name__");
                    // TODO: Review fragile namespacing
                    string x = $"signature = inspect.signature({moduleName}.{attrName})";
                    scope.Exec(x);
                    var signature = scope.Get("signature");
                    var name = attr.GetAttr("__name__").ToString();
                    methods.Add(MethodReflection.FromMethod(signature, name, moduleName.ToString()));
                    callables.Add(attr.ToString());
                }
            }
            return methods;
        }

        public static string Compile(this List<MethodDeclarationSyntax> methods)
        {
            StringWriter sw = new StringWriter();
            foreach (var method in methods)
            {
                method.NormalizeWhitespace().WriteTo(sw);
            }
            return sw.ToString();
        }
    }
}
