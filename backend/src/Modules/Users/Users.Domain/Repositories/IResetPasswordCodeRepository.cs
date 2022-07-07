using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Users;
using Users.Domain.Auth;

namespace Users.Domain.Repositories
{
    public interface IResetPasswordCodeRepository
    {
        ResetCodeRepresentation CreateResetPasswordCode(ResetCodeRepresentation resetCodeRepresentation);
        void UpdateResetPasswordCode(ResetCodeRepresentation resetCodeRepresentation);
        void RemoveResetPasswordCode(ResetCode resetCode, string email);
        void RemoveResetCodesByEmail(string email);
        ResetCodeRepresentation FindResetPasswordCode(ResetCode resetCode, string email);
        int CountResetCodesForEmail(string email);
    }
}
