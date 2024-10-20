﻿using Common.Application.Commands;
using System.ComponentModel.DataAnnotations;

namespace Users.Application.Commands.SignIn
{
    public class SignInCommand : ICommand
    {
        [Required]
        public string Username { get; }
        [Required]
        public string Password { get; }

        public SignInCommand(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
