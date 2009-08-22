using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using Vestris.ResourceLib;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLibUnitTests
{
    [TestFixture]
    public class VersionInfoTests
    {
        [Test]
        public void TestLoad()
        {
            string filename = Path.Combine(Environment.SystemDirectory, "write.exe");
            Assert.IsTrue(File.Exists(filename));
            using (ResourceInfo vi = new ResourceInfo())
            {
                vi.Load(filename);

                foreach (string type in vi.ResourceTypes)
                {
                    Console.WriteLine(type);
                    foreach (Resource resource in vi.Resources[type])
                    {
                        Console.WriteLine(" {0} ({1}) - {2} byte(s)", resource.Name, resource.Language, resource.Size);

                        if (resource is VersionResource)
                        {
                            VersionResource versionResource = (VersionResource)resource;
                            Console.WriteLine("  File version: {0}", versionResource.FileVersion);
                            Console.WriteLine("  Language: {0}", versionResource.Language);
                            Dictionary<string, ResourceTable>.Enumerator stringTableResourceEnumerator = versionResource.Resources.GetEnumerator();
                            while (stringTableResourceEnumerator.MoveNext())
                            {
                                Console.WriteLine("Dump of {0}:", stringTableResourceEnumerator.Current.Key);
                                DumpResource.Dump(stringTableResourceEnumerator.Current.Value);
                            }
                        }
                        else if (resource is GroupIconResource)
                        {
                            DumpResource.Dump(resource as GroupIconResource);
                        }
                        else
                        {
                            DumpResource.Dump(resource);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestCustom()
        {
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string filename = Path.Combine(Path.GetDirectoryName(HttpUtility.UrlDecode(uri.AbsolutePath)), "Binaries\\custom.exe");
            Assert.IsTrue(File.Exists(filename));
            using (ResourceInfo vi = new ResourceInfo())
            {
                vi.Load(filename);
                // version resources (well-known)
                List<Resource> versionResources = vi.Resources["16"]; // RT_VERSION
                Assert.IsNotNull(versionResources);
                Assert.AreEqual(1, versionResources.Count);
                Resource versionResource = versionResources[0];
                Assert.AreEqual(versionResource.Name, "1");
                Assert.AreEqual(versionResource.Type, "16");
                // custom resources
                List<Resource> customResources = vi.Resources["CUSTOM"];
                Assert.IsNotNull(customResources);
                Assert.AreEqual(1, customResources.Count);
                Resource customResource = customResources[0];
                // check whether the properties are string representations
                Assert.AreEqual(customResource.Name, "RES_CONFIGURATION");
                Assert.AreEqual(customResource.Type, "CUSTOM");
            }
        }
    }
}
