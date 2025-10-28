import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
    getApiUrl() : string{
        return 'https://localhost:7077';
    }
}
