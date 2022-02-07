import { environment } from "../../../environments/environment";

export function isDemoModeDisabled() {
    if(!environment.serverDemoModeEnabled){
        return true;
    }
    const cookieMatch = document.cookie.match(/^.*demoModeDisabled\=true.*$/);
    return cookieMatch && cookieMatch.length > 0;     
}