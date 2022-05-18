import { Token } from '@angular/compiler';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TokenResponse } from '../responses/token-response';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class TokenService {

  private AT_TOKEN = 'AT';
  private RF_TOKEN = 'RT';
  private USER_ID = 'ID';
  private FIRST_NAME = 'FN';

  constructor(private userService : UserService) { }

  saveSession(tokenResponse : TokenResponse)
  {
    localStorage.setItem(this.AT_TOKEN, tokenResponse.accessToken);
    localStorage.setItem(this.RF_TOKEN, tokenResponse.refreshToken);
    if(tokenResponse.userId)
    {
      localStorage.setItem(this.USER_ID, tokenResponse.userId.toString());
      localStorage.setItem(this.FIRST_NAME, tokenResponse.firstName);
    }
  }

  getSession() : TokenResponse | null
  {
    if(localStorage.getItem(this.AT_TOKEN)){
      const tokenRepose: TokenResponse =
      {
        accessToken: window.localStorage.getItem('AT') || '',
        refreshToken: window.localStorage.getItem('RT') || '',
        firstName: window.localStorage.getItem('FN') || '',
        userId: +(window.localStorage.getItem('ID') || 0),
      }
      return tokenRepose;
    }
    return null;
  }

  logout()
  {
    localStorage.clear();
  }

  isLoggedIn() : boolean
  {
    let session = this.getSession();
    if(!session)
    {
      return false;
    }

    //check if otken is expired

    const jwtToken = JSON.parse(atob(session.accessToken.split('.')[1]));

    const tokenExpired = Date.now() > (jwtToken.exp * 1000);

    return !tokenExpired;
  }

  refreshToken (session : TokenResponse) : Observable<TokenResponse>
  {
    return this.userService.refreshToken(session);
  }
}
