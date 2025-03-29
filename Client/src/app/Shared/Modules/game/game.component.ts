import {Component, HostListener, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {GameService} from "../../Services/game.service";
import {Subscription} from "rxjs";
import {FormsModule} from "@angular/forms";
import {NgForOf, NgIf} from "@angular/common";
import {Player} from "../../Interfaces/Player";
import {PlayerScore} from "../../Interfaces/PlayerScore";
import {TopPlayerComponent} from "../top-player/top-player.component";
import {RegisterGameComponent} from "../register-game/register-game.component";
import {ErrorComponent} from "../error/error.component";

@Component({
  selector: 'app-game',
  imports: [
    FormsModule,
    NgIf,
    TopPlayerComponent,
    RegisterGameComponent,
    ErrorComponent,
  ],
  templateUrl: './game.component.html',
  styleUrl: './game.component.css'
})
export class GameComponent implements OnInit, OnDestroy {
  private gameSubscription: Subscription | undefined;
  playerName: string | null = null;
  isNameEntered: boolean = false;
  players: Player[] = [];
  playerScores: PlayerScore[] = [];
  errorMessage: string | null = null;
  info: string | null = null;
  private pressedKeys: Set<string> = new Set();
  @ViewChild(RegisterGameComponent) registerGameComponent: RegisterGameComponent | undefined;

  constructor(private gameService: GameService) {
  }

  ngOnInit() {
    this.gameService.startConnection();
    this.gameService.checkGame();

    const canvas = document.getElementById('gameCanvas') as HTMLCanvasElement;
    const ctx = canvas.getContext('2d')!;

    const starImage = new Image();
    starImage.src = '/star.png';

    const carImage = new Image();
    carImage.src = '/car.png';

    let imagesLoaded = 0;
    const totalImages = 2;

    const checkImagesLoaded = () => {
      imagesLoaded++;
      if (imagesLoaded === totalImages) {
        this.startGameLoop(ctx, carImage, starImage, canvas);
      }
    };

    starImage.onload = checkImagesLoaded;
    carImage.onload = checkImagesLoaded;
  }

  private startGameLoop(ctx: CanvasRenderingContext2D, carImage: HTMLImageElement, starImage: HTMLImageElement, canvas: HTMLCanvasElement) {
    this.gameSubscription = this.gameService.players.subscribe(players => {
      this.players = players.filter(p => p.name);
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      this.players.forEach(player => {
        ctx.drawImage(carImage, player.x, player.y, 40, 40);
        ctx.fillStyle = 'black';
        ctx.font = '14px Arial';
        ctx.fillText(player.name, player.x, player.y - 5);
      });

      this.gameService.star.subscribe(star => {
        if (star) {
          ctx.drawImage(starImage, star.x - 10, star.y - 10, 30, 30);
        }
      });

      this.gameService.playerScores.subscribe(playerScores => {
        this.playerScores = playerScores;
      });

      this.gameService.info.subscribe(info => {
        this.info = info;
        if (this.info === 'full' && this.registerGameComponent) {
          this.registerGameComponent.onDisconnect();
        }
      })

      this.gameService.errorMessage.subscribe((message) => {
        if (message) {
          this.errorMessage = message;
        }
      });
    });
  }

  ngOnDestroy(): void {
    if (this.gameSubscription) {
      this.gameSubscription.unsubscribe();
    }
    this.gameService.leaveGame();
  }

  receivePlayerName(playerName: string | null) {
    this.playerName = playerName;
  }

  receiveIsNameEntered(isNameEntered: boolean) {
    this.isNameEntered = isNameEntered;
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent) {
    const keyMap: { [key: string]: string } = {
      ArrowUp: 'up',
      ArrowDown: 'down',
      ArrowLeft: 'left',
      ArrowRight: 'right',
      w: 'up',
      s: 'down',
      a: 'left',
      d: 'right'
    };

    const direction = keyMap[event.key];
    if (direction) {
      this.pressedKeys.add(direction);
      this.move();
    }
  }

  @HostListener('window:keyup', ['$event'])
  handleKeyUp(event: KeyboardEvent) {
    const keyMap: { [key: string]: string } = {
      ArrowUp: 'up',
      ArrowDown: 'down',
      ArrowLeft: 'left',
      ArrowRight: 'right',
      w: 'up',
      s: 'down',
      a: 'left',
      d: 'right'
    };

    const direction = keyMap[event.key];
    if (direction) {
      this.pressedKeys.delete(direction);
      this.move();
    }
  }

  private move() {
    this.gameService.move(Array.from(this.pressedKeys));
  }
}
