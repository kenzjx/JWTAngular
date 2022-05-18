import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { LoginRequest } from '../requests/login-request';
import { RefreshTokenRequest } from '../requests/refresh-token-request';
import { SignupRequest } from '../requests/signup-request';
import { TokenResponse } from '../responses/token-response';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private httpClient : HttpClient) { }

  login(loginRequest : LoginRequest) : Observable<any>
  {
    return this.httpClient.post(`${environment.apiUrl}/users/login`, loginRequest);
  }

  signup(SignupRequest : SignupRequest)
  {
    return this.httpClient.post(`${environment.apiUrl}/users/signup`, SignupRequest, {responseType: 'text'});
  }

  refreshToken(session : RefreshTokenRequest) : Observable<TokenResponse>
  {
    let refreshTokenRequest : any = {
      UserId : session.userId,
      RefreshToken : session.refreshToken
    }
    return this.httpClient.post<TokenResponse>(`${environment.apiUrl}/users/refreshtoken`, refreshTokenRequest);
  }

  logout()
  {
    return this.httpClient.post(`${environment.apiUrl}/users/signup`, null);
  }

  getUserInfo() : Observable<any>
  {
    return this.httpClient.get(`${environment.apiUrl}/users/info`);
  }
}
