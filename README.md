# Coin Catcher 3D

Proyek ini dibuat untuk memenuhi tugas mata kuliah pembuatan *game* menggunakan **Unity Engine**.

**Coin Catcher 3D** adalah *game arcade* sederhana bergaya *low-poly* 3D. Di dalam permainan ini, pemain menggerakkan karakter bernama **Kai** di sebuah arena tertutup untuk menangkap koin yang jatuh dari langit sebanyak mungkin sambil menghindari bom.

## Konsep Permainan

Pemain memulai permainan dengan **3 nyawa**. Sepanjang permainan, koin dan bom akan terus jatuh secara acak di dalam area pagar pembatas:

- **Koin**: Memberikan +10 poin setiap kali diambil. Koin yang sudah menyentuh tanah akan bertahan selama 2 detik sebelum menghilang.
- **Bom**: Mengurangi 1 nyawa jika mengenai pemain atau meledak saat menyentuh pemain. Permainan berakhir (*Game Over*) ketika nyawa habis.
- **Tingkat Kesulitan**: Kecepatan jatuh objek serta frekuensi kemunculan koin dan bom akan meningkat secara bertahap seiring waktu permainan berjalan.
- **Rintangan & NPC**: Di dalam arena terdapat sejumlah batu rintangan yang disusun di awal permainan, serta dua karakter NPC (**Maya** dan **Pak Arka**) yang sesekali memunculkan dialog singkat sesuai kondisi permainan (saat mulai, mendapat *streak* koin, terkena bom, atau mencetak rekor baru).

## Kontrol

Permainan mendukung input *Keyboard* maupun *Gamepad* (menggunakan Unity Input System):

| Aksi | Keyboard | Gamepad |
| :--- | :--- | :--- |
| Bergerak | `W`, `A`, `S`, `D` atau Tombol Panah | *Left Stick* |
| *Dash* (Menghindar) | `Left Shift` / `Right Shift` | Tombol `A` / *Cross* (*Button South*) |

> **Catatan**: Saat melakukan *dash*, karakter bergerak lebih cepat dan kebal sementara terhadap ledakan bom, namun terdapat jeda waktu (*cooldown*) selama 1,5 detik sebelum dapat digunakan kembali.

## Struktur Direktori

```text
Assets/
├── Audio/              # Berkas musik latar (BGM) dan efek suara (SFX)
├── Editor/             # Skrip bantuan untuk setup di Unity Editor
├── GeneratedMeshes/    # Mesh hasil ekstraksi model 3D
├── Materials/          # Material objek, karakter, dan lingkungan
├── Prefabs/            # Prefab koin, bom, efek partikel, dan properti level
├── Resources/          # Prefab rintangan (Obstacles) yang dimuat saat runtime
├── Scenes/             # MainMenu.unity dan MainScene.unity
├── Scripts/            # Skrip C# logika permainan
├── Settings/           # Konfigurasi Universal Render Pipeline (URP)
├── Sprites/            # Aset UI 2D
└── ThirdParty/         # Aset model 3D dari pihak ketiga
```

### Skrip Utama (`Assets/Scripts/`)

- `GameManager3D.cs` — Mengatur alur permainan (*Main Menu*, hitung mundur, *Playing*, *Game Over*), skor, nyawa, dan penyimpanan *High Score* melalui `PlayerPrefs`.
- `Player3DController.cs` & `PlayerAnimator.cs` — Mengatur pergerakan karakter, batas arena, mekanika *dash*, serta animasi karakter.
- `ItemSpawner3D.cs` & `FallingItem3D.cs` — Mengatur titik muncul serta kecepatan jatuh koin dan bom.
- `ObjectPooler3D.cs` — Mengelola *object pooling* untuk koin, bom, dan partikel ledakan agar objek tidak perlu dibuat dan dihapus berulang kali.
- `ObstacleManager.cs` & `ObstacleVisual.cs` — Mengatur penempatan batu rintangan di arena dan efek goyang saat ditabrak.
- `DialogManager.cs`, `DialogBubble.cs`, & `NPC3DController.cs` — Mengatur kemunculan balon dialog dan reaksi NPC.
- `UIManager3D.cs`, `MainMenuManager.cs`, & `GameOverUI.cs` — Mengelola tampilan antarmuka pengguna (HUD, menu utama, jeda, dan layar akhir).
- `AudioManager.cs` — Mengelola pemutaran musik latar dan efek suara.
- `CameraFollow3D.cs` — Mengatur kamera agar mengikuti posisi pemain beserta efek guncangan saat terkena bom.

## Cara Membuka Proyek

1. Kloning repositori ini:
   ```bash
   git clone https://github.com/ikhsanbskr/coin-catcher.git
   ```
2. Buka **Unity Hub**, pilih **Add -> Add project from disk**, lalu arahkan ke folder repositori ini.
3. Buka proyek menggunakan **Unity 6 (`6000.6.3f1`)** (Universal Render Pipeline).
4. Buka *scene* `Assets/Scenes/MainMenu.unity`, lalu tekan tombol **Play** di Unity Editor.

## Kredit Aset

- *Ultimate Nature Pack* oleh Quaternius (CC0)
- *Starter Kit 3D Platformer* oleh Kenney (CC0)
