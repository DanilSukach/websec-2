import {Component, Input, OnInit} from '@angular/core';
import {NgForOf, NgIf} from "@angular/common";
import {GameService} from "../../Services/game.service";
import {PlayerScore} from "../../Interfaces/PlayerScore";

@Component({
  selector: 'app-top-player',
  imports: [
    NgForOf,
    NgIf
  ],
  templateUrl: './top-player.component.html',
  styleUrl: './top-player.component.css'
})
export class TopPlayerComponent{
  @Input() playerScores!: PlayerScore[];
}
