
var strong = /(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z])(?!.*[\!\@\#\$\%\^\&\*\+\=\)\(\_\`\~\'\"\,\.\|])(?=.{8,})/;
var medium = /(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z])(?!.*[\!\@\#\$\%\^\&\*\+\=\)\(\_\`\~\'\"\,\.\|])(?=.{8,})/;
var weak = /(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z])(?!.*[\!\@\#\$\%\^\&\*\+\=\)\(\_\`\~\'\"\,\.\|])(?=.{8,})/;


export class PasswordStrength {
  static measure(password: string): string {
    if (password == null) {
      throw Error("Null arg");
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
