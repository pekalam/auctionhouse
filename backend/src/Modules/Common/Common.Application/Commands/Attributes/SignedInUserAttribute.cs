using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Common.Application.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SignedInUserAttribute : Attribute
    {
    }
}