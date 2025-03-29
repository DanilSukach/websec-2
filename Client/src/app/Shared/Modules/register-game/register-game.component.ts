import {Component, EventEmitter, Output} from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NgIf} from "@angular/common";
import {Subscription} from "rxjs";
import {Player} from "../../Interfaces/Player";
import {PlayerScore} from "../../Interfaces/PlayerScore";
import {GameService} from "../../Services/game.service";

@Component({
  selector: 'app-register-game',
  imports: [
    FormsModule,
    NgIf
  ],
  templateUrl: './register-game.component.html',
  styleUrl: './register-game.component.css'
})
export class RegisterGameComponent {
  @Output() playerNameChange = new EventEmitter<string | null>();
  @Output() isNameEnteredChange = new EventEmitter<boolean>();

  playerName: string | null = null;
  isNameEntered: boolean = false;

  constructor(private gameService: GameService) {}

  onDisconnect() {
    this.gameService.leaveGame();
    this.isNameEntered = false;
    this.playerNameChange.emit(this.playerName);
    this.isNameEnteredChange.emit(this.isNameEntered);
  }

  onNameEntered() {
    if (!this.playerName?.trim()) {
      return;
    }

    this.gameService.joinGame(this.playerName);
    this.isNameEntered = true;
    this.playerNameChange.emit(this.playerName);
    this.isNameEnteredChange.emit(this.isNameEntered);
  }
}
