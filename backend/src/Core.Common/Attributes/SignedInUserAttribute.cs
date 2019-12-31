using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Core.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SignedInUserAttribute : Attribute
    {
    }
}