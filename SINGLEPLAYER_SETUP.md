# Konfiguracja Singleplayer - Instrukcja Ustawienia

## Przegląd
Dodałem wsparcie dla trybu singleplayer do Twojej gry 2D. **Logika singleplayera jest teraz zintegrowana w RelayNetworkManager** - nie potrzebujesz osobnego managera!

## Czego Zmienione

### Zmodyfikowane Pliki
- **RelayNetworkManager.cs** - Dodana metoda `StartSingleplayer()` (logika singleplayera)
- **Connect.cs** - Dodana metoda `Singleplayer()` 
- **IsoWorldGenerator.cs** - Obsługa inicjalizacji offline
- **IsoPlayerController.cs** - Sterowanie postacią bez sieciowania
- **PlayerCameraSetter.cs** - Kamera w trybie singleplayer

## Kroki Ustawienia w Unity (PROSTY)

### Krok 1: Edytuj GameObject RelayNetworkManager
1. Otwórz scene `Assets/Scenes/SampleScene.unity`
2. Znajdź GameObject **"RelayNetworkManager"** w hierarchii (powinien już istnieć)
3. W Inspectorze tego GameObject'u, znajdź component **RelayNetworkManager**
4. Przypisz dwa pola:
   - **Player Prefab**: Przeciągnij `Assets/Prefabs/Player/Player.prefab`
   - **Spawn Point**: (Opcjonalne) Transform gdzie ma się spawner gracz

### Krok 2: Dodaj Przycisk Singleplayer do UI
1. Znajdź canvas z przyciskami Host/Client
2. Dodaj nowy przycisk "Singleplayer"
3. W komponencie Button, w sekcji On Click():
   - Dodaj nowe event
   - Przeciągnij GameObject z componentem Connect
   - Z dropdown wybierz: `Connect.Singleplayer()`

### Krok 3: Testuj
1. W edytorze Play Mode kliknij przycisk "Singleplayer"
2. Powinieneś zobaczyć:
   - Postać spawnu się w grze
   - Możliwość ruchu postacią (WASD lub strzałki)
   - Świat generuje się wokół postaci
   - Kamera podąża za postacią

## Obsługiwane Funkcje

✅ Tryb singleplayer (bez networkingu)
✅ Lokalna generacja świata
✅ Sterowanie postacią
✅ Kamera śledząca
✅ Zoomowanie kamery (scroll myszy)
✅ Zachowywanie kompatybilności multiplayer

## Troubleshooting

**Problem**: "Player prefab not assigned" w console
- **Rozwiązanie**: Przypisz Player.prefab w Inspectorze RelayNetworkManager

**Problem**: Świat się nie generuje
- **Rozwiązanie**: Upewnij się że IsoWorldGenerator jest w scenie

**Problem**: Multiplayer nie działa
- **Rozwiązanie**: Multiplayer powinien wciąż działać normalnie, sprawdź czy Host/Client przyciski działają

## Architektura

System wykorzystuje:
- **RelayNetworkManager.StartSingleplayer()** - Inicjalizuje grę bez networkingu
- Reflection (Type.GetType) - Znalezienie IsoWorldGenerator bez zapamiętywania referencji
- Dynamic typing - Wywoływanie metod na obiektach znalezionych dynamicznie
- Flagi isSinglePlayer - Kontrola behaviour w każdym komponencie

Brak potrzeby SingleplayerManager'a - wszystko działa z RelayNetworkManager!
