import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {BehaviorSubject} from "rxjs";
import {Star} from "../Interfaces/Star";
import {Player} from "../Interfaces/Player";
import {PlayerScore} from "../Interfaces/PlayerScore";
import {environment} from "../../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private hubConnection!: signalR.HubConnection;
  players = new BehaviorSubject<Player[]>([]);
  star = new BehaviorSubject<Star | null>(null);
  playerScores = new BehaviorSubject<PlayerScore[]>([]);
  errorMessage = new BehaviorSubject<string | null>(null);
  info = new BehaviorSubject<string | null>(null);
  private isConnected = false;

  async startConnection(): Promise<void> {
    if (this.isConnected) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/gameHub`)
      .withAutomaticReconnect()
      .build();

    try {
      await this.hubConnection.start();
      this.isConnected = true;
      this.setupListeners();
    } catch (error) {
      console.error("Connection failed:", error);
      this.errorMessage.next("Ошибка соединения с сервером.");
    }
  }

  private setupListeners(): void {
    this.hubConnection.on('StarCollected', (star: Star) => {
      this.star.next(star);
    });

    this.hubConnection.on('GameState', (players: Player[]) => {
      this.players.next(players);
    });

    this.hubConnection.on('TopPlayers', (topPlayers: PlayerScore[]) => {
      this.playerScores.next(topPlayers);
    });

    this.hubConnection.on('Info', (info: string) => {
      this.info.next(info);
    })
  }

  async joinGame(name: string): Promise<void> {
    if (!this.isConnected) {
      await this.startConnection();
    }
    try {
      await this.hubConnection.invoke('RegisterPlayer', name);
    } catch (error) {
      console.error("Join game error:", error);
      this.errorMessage.next("Ошибка входа в игру.");
    }
  }

  async move(direction: string[]): Promise<void> {
    if (!this.isConnected) return;
    try {
      await this.hubConnection.send('Move', direction);
    } catch (error) {
      console.error("Move failed:", error);
    }
  }

  async checkGame(): Promise<void> {
    if (!this.isConnected) {
      await this.startConnection();
    }
    try {
      await this.hubConnection.invoke('CheckGame');
    } catch (error) {
      console.error("Check game error:", error);
      this.errorMessage.next("Ошибка просмотра игры.");
    }
  }

  async leaveGame(): Promise<void> {
    if (!this.isConnected || !this.hubConnection) return;

    try {
      this.isConnected = false;
      await this.hubConnection.invoke('LeaveGame');
    } catch (error) {
      console.error("Error notifying server about leaving game:", error);
    }
  }
}
