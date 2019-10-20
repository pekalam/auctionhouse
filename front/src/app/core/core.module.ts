import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServerMessageService } from './services/ServerMessageService';
import { SignUpCommand } from './commands/SignUpCommand';
import { SignInCommand } from './commands/SignInCommand';
import { AuctionQuery } from './queries/AuctionQuery';
import { AuctionsQuery } from './queries/AuctionsQuery';
import { BidCommand } from './commands/BidCommand';
import { AuthenticationStateService } from './services/AuthenticationStateService';

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  providers: [SignUpCommand, SignInCommand, AuctionQuery, AuctionsQuery, BidCommand,
    ServerMessageService, AuthenticationStateService]
})
export class CoreModule { }
