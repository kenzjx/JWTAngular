import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
  HTTP_INTERCEPTORS
} from '@angular/common/http';
import { catchError, EMPTY, Observable, tap, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';
import { ErrorResponse } from '../responses/error-response';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

   isRefreshingToken : boolean = false;

  constructor(private tokenService : TokenService, private userService : UserService, private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      tap(response => console.log(JSON.stringify(response))),
      catchError((error : HttpErrorResponse) => {
        console.log(JSON.stringify(error));
        let session = this.tokenService.getSession();
        if(error.status === 401 && session != null && this.tokenService.isLoggedIn() && !this.isRefreshingToken)
        {
          this.isRefreshingToken = true;
          console.log('Acces Token is expired, we need to renew it');
          this.userService.refreshToken(session).subscribe({
            next: data =>{
              console.info('Tokens renew, we willl save them into the local storage');
              this.tokenService.saveSession(data);
            },
            error: () =>{

            },
            complete: () => {this.isRefreshingToken = false}
          })
        }
        else if(error.status === 400 && error.error.errorCode == 'invalid_grant') {
          console.log('the refresh token expired, the user must login agian');
          this.tokenService.logout();
          this.router.navigate(['login']);
        }
        else
        {
          let erroReponse: ErrorResponse = error.error;
          console.log(JSON.stringify(erroReponse));
          return throwError(() => erroReponse);
        }
        return EMPTY;
      })
    );
  }
}

export const ErrorInterceptorProvider = {provide: HTTP_INTERCEPTORS, userClass: ErrorInterceptor, multi: true};
