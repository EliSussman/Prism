﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Prism.Injector
{
    public static class InjectionExample
    {
        public static void Example()
        {
            var c = new CecilContext("Terraria.exe");
            var r = c.Resolver; // this contains a lot of useful methods

            var toInjectIn = c.PrimaryAssembly;

            var writeStrLine = r.MethodOfA<string>(Console.WriteLine);

            // get the update method
            var mainUpdate = r.GetType("Terraria.Main").GetMethod("Update");
            // Console.WriteLine("Hello, world") in IL
            Instruction[] hw = new[] // TODO: make this easier to do, too (emitter class, fluent pattern?)
            {
                // these instructions are hardcoded, but ILSnippetCompiler can be used as well (but snippets should be embedded resources or something, not directly embedded in the source)
                // references to prism and (unpatched) terraria must be passed to the compilesnippet(s) methods

                // ldstr "Hello, world"
                Instruction.Create(OpCodes.Ldstr, "Hello, world"),
                // call void [mscorlib]System.Console::WriteLine(string)
                Instruction.Create(OpCodes.Call , writeStrLine  )
            };

            ILInjector.Inject(toInjectIn, new InjectionData[]
            {
                // this injects 'hello world' in 3 places
                // (also see all XmlDoc in InjectionData.cs)

                // inject it in the very beginning of Main.Update
                InjectionData.Method.NewMethodPre(mainUpdate, hw),
                // inject it after the 5th instruction
                InjectionData.Instruction.NewInstructionIndex(mainUpdate, InjectionPosition.Post, hw, 5),

                // inject it after the first occurence of a "String.Concat(object[])" call
                // this throws an exception, because there is no String.Concat call in Update, but I hope you get how it works
                //InjectionData.Call.NewCall(mainUpdate, InjectionPosition.Post, hw, r.MethodOfF<object[], string>(String.Concat))
            });

            toInjectIn.Write("Prism.Terraria.dll");
        }
    }
}
