﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Prism
{
    static class Program
    {
        static Assembly[] DllContainingAssemblies =
        {
            Assembly.Load(AssemblyName.GetAssemblyName("Prism.Terraria.dll")), // can't use typeof(...).Assembly here, see comment in Main
            Assembly.GetExecutingAssembly()
        };

        static Assembly ResolveFromResources(Assembly container, string name)
        {
            string[] allNames = container.GetManifestResourceNames();

            string
                fileName = name + ".dll",
                fullName = Array.Find(allNames, e => e.EndsWith(fileName, StringComparison.Ordinal)),

                pdbName = Path.ChangeExtension(fileName, ".pdb"),
                fullPdbName = Array.Find(allNames, e => e.EndsWith(pdbName, StringComparison.Ordinal)); // null if not found

            if (String.IsNullOrEmpty(fullName))
                return null;

            using (Stream dllStream = container.GetManifestResourceStream(fullName))
            {
                byte[] assembly = new byte[dllStream.Length];
                dllStream.Read(assembly, 0, assembly.Length);

                // load PDB if exists
                if (!String.IsNullOrEmpty(fullPdbName))
                    using (Stream pdbStream = container.GetManifestResourceStream(fullPdbName))
                    {
                        byte[] pdb = new byte[pdbStream.Length];
                        pdbStream.Read(pdb, 0, pdb.Length);

                        return Assembly.Load(assembly, pdb);
                    }

                return Assembly.Load(assembly);
            }
        }
        static void Init()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            AppDomain.CurrentDomain.AssemblyResolve += (_, rea) =>
            {
                string displayName = new AssemblyName(rea.Name).Name;
                if (displayName == "Terraria")
                    return DllContainingAssemblies[0];

                // LINQ uses lazy evaluation, thus it doesn't try to load the assembly from all assemblies in DllContainingAssemblies,
                // but the loop breaks when the predicate in Find returns true.
                return DllContainingAssemblies.Select(c => ResolveFromResources(c, displayName)).First(a => a != null);
            };
        }

        static void Main(string[] args)
        {
            try
            {
                Init();

                TerrariaLauncher.Launch(); // having it in this class would cause a TypeInitializationException before Main is called.
                                           // this happens because all types in Prism.Terraria.dll still refer to Terraria.exe, but this is
                                           // fixed in Init() by resolving Terraria.exe as Prism.Terraria.dll.
            }
            catch (Exception e)
            {
                ExceptionHandler.HandleFatal(e);
            }
        }
    }
}
