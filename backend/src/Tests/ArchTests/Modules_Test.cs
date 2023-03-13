//using Core.Common.Domain;
//using Core.Query.EventHandlers;
//using NetArchTest.Rules;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Xunit;
//using Xunit.Abstractions;

//namespace Test.Arch
//{
//    public class Modules_Test
//    {
//        private string[] _modules;
//        private ITestOutputHelper _output;

//        public Modules_Test(ITestOutputHelper output)
//        {
//            _output = output;
//            _modules = new[] { "Auctions", "Users", "UserPayments", "AuctionBids" };
//        }

//        [Fact]
//        public void Interfaces_and_abstract_classed_are_implemented_in_solution_projects()
//        {
//            var adapterAssemblies = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
//                .Where(n => n.EndsWith("dll") && (n.Contains("Adapter") || n.Contains("Auctionhouse.Command")))
//                .Select(f => f.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)[^1])
//                .Select(f => f.Replace(".dll", ""));

//            List<Type> allTypesToImplement = new();

//            foreach (var module in _modules)
//                foreach (var suffix in new[] { ".Domain", ".Application" })
//                {
//                    var moduleName = module + suffix;

//                    var typesToImplement = Types.InAssembly(Assembly.Load(moduleName))
//                        .That()
//                        .AreInterfaces()
//                        .Or()
//                        .AreAbstract()
//                        .And()
//                        .AreNotStatic()
//                        .And()
//                        .AreNotSealed()
//                        .GetTypes().ToArray();

//                    allTypesToImplement.AddRange(typesToImplement);
//                }



//            foreach (var adapter in adapterAssemblies)
//            {
//                List<Type> toRemove = new();
//                foreach (var toImplement in allTypesToImplement)
//                {
//                    var implementingNotPublic = Types.
//                        InAssembly(Assembly.Load(adapter))
//                        .That()
//                        .AreNotPublic()
//                        .And()
//                        .ImplementInterface(toImplement)
//                        .GetTypes().ToList();
//                    var implementingPublic = Types.
//                        InAssembly(Assembly.Load(adapter))
//                        .That()
//                        .ArePublic()
//                        .And()
//                        .ImplementInterface(toImplement)
//                        .GetTypes().ToList();
//                    implementingNotPublic.AddRange(implementingPublic);

//                    if (implementingNotPublic.Any())
//                    {
//                        toRemove.Add(toImplement);
//                    }
//                }
//                allTypesToImplement.RemoveAll(t => toRemove.Contains(t));
//            }


//            if (allTypesToImplement.Count == 0)
//            {
//                return;
//            }
//            _output.WriteLine("Not implemented in any of adapter projects:");
//            foreach (var type in allTypesToImplement)
//            {
//                _output.WriteLine(type.AssemblyQualifiedName);
//            }
//            Assert.False(true);
//        }


//        [Fact]
//        public void f()
//        {
//            var adapterAssemblies = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
//                .Where(n => n.EndsWith("dll") && (n.Contains("Adapter") || n.Contains("Auctionhouse.Command")))
//                .Select(f => f.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)[^1])
//                .Select(f => f.Replace(".dll", ""));

//            var eventsToHandle = new List<Type>();

//            foreach (var module in _modules)
//                foreach (var suffix in new[] { ".Domain", ".Application", ".DomainEvents" })
//                {
//                    var moduleName = module + suffix;

//                    var events = Types.InAssembly(Assembly.Load(moduleName))
//                        .That()
//                        .Inherit(typeof(Event))
//                        .GetTypes().ToArray();

//                    eventsToHandle.AddRange(events);
//                }

//            var eventConsumers = (new[] { "ReadModel.Core" })
//                .SelectMany(moduleName => Types.InAssembly(Assembly.Load(moduleName))
//                                    .That()
//                                    .Inherit(typeof(EventConsumer<,>))
//                                    .GetTypes())
//                .ToArray();


//            foreach (var eventConsumer in eventConsumers)
//            {
//                var handledEvent = eventConsumer.BaseType.GenericTypeArguments[0];
//                eventsToHandle.Remove(handledEvent);
//            }

//            foreach (var eventToHandle in eventsToHandle)
//            {
//                _output.WriteLine("Not handled in read model: " + eventToHandle.Name);
//            }
//            Assert.False(eventsToHandle.Count > 0);
//        }
//    }
//}