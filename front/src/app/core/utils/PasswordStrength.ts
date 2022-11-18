let strong = /(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z])(?=.*[\!\@\#\$\%\^\&\*\+\=\)\(\_\`\~\'\"\,\.\|])(?=.{8,})/;
let medium = /(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z])(?!.*[\!\@\#\$\%\^\&\*\+\=\)\(\_\`\~\'\"\,\.\|])(?=.{4,})/;
let weak = /(?=.*[a-z])(?!.*[\!\@\#\$\%\^\&\*\+\=\)\(\_\`\~\'\"\,\.\|])(?=.{4,})/;


export class PasswordStrength {
  static measure(password: string): string {
    if (password == null) {
      throw Error('Null arg');
    }

    if (password.match(strong)) {
      return 'strong';
    }
    if (password.match(medium)) {
      return 'medium';
    }
    if (password.match(weak)) {
      return 'weak';
    }
    return null;
  }
}
