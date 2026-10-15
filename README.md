# 🪙 Coin Catcher 3D

![Unity](https://img.shields.io/badge/Unity-6000.6.3f1-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-Scripting-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Render Pipeline](https://img.shields.io/badge/URP-Universal_Render_Pipeline-blue?style=for-the-badge)
![Status](https://img.shields.io/badge/Project-Tugas_Kuliah-FBC437?style=for-the-badge)

**Coin Catcher 3D** adalah gim *arcade* 3D bergaya *low-poly* yang dikembangkan menggunakan **Unity Engine (Universal Render Pipeline)**. Proyek ini dibuat untuk memenuhi **tugas mata kuliah Pembuatan / Pengembangan Gim (*Game Development*)** di perkuliahan.

Dalam gim ini, pemain mengendalikan karakter utama bernama **Kai** di sebuah arena terbuka untuk mengumpulkan koin sebanyak mungkin sambil menghindari bom yang jatuh dari langit serta memanfaatkan rintangan di sekitar arena.

---

## 🎯 Latar Belakang Proyek Kuliah

Proyek **Coin Catcher 3D** dirancang sebagai implementasi langsung dari konsep-konsep dasar hingga menengah dalam pengembangan gim 3D menggunakan Unity dan C#, meliputi:

1. **Arsitektur *Game State* & *Singleton Pattern*** — Pengelolaan alur permainan (*Main Menu*, *Countdown*, *Playing*, *Pause*, dan *Game Over*) secara terpusat melalui `GameManager3D`.
2. **Optimasi Performa dengan *Object Pooling*** — Penggunaan kembali objek koin, bom, dan efek ledakan (*particle VFX*) melalui `ObjectPooler3D` tanpa membebani memori akibat *instantiation* dan *garbage collection* berulang.
3. **Peningkatan Kesulitan Dinamis (*Dynamic Difficulty Scaling*)** — Kecepatan jatuh objek, interval kemunculan (*spawn rate*), dan jumlah maksimum koin maupun bom meningkat secara bertahap seiring waktu permainan berjalan.
4. **Tata Letak Arena Prosedural (*Procedural Obstacle Generation*)** — Penempatan rintangan batu dan objek lingkungan secara acak berbasis *seed* di empat kuadran arena melalui `ObstacleManager`.
5. **Interaksi NPC & Umpan Balik Visual (*Game Feel / Juice*)** — Kehadiran karakter pendukung (**Maya** dan **Pak Arka**) dengan balon dialog kontekstual, efek *camera shake* saat terkena ledakan, perubahan *Field of View* (FOV) serta *trail effect* saat melakukan *dash*, hingga animasi objek melayang.

---

## ✨ Fitur Utama

- **Mekanika *Dash* dengan *Invulnerability***: Pemain dapat melakukan *dash* cepat untuk mengejar koin atau menembus ledakan bom tanpa kehilangan nyawa, dilengkapi *cooldown* dan indikator antarmuka.
- **Sistem Kesulitan Progresif**: Semakin lama bertahan, frekuensi dan kecepatan jatuhnya koin serta bom semakin menantang.
- **Dialog Interaktif NPC**: **Maya** dan **Pak Arka** memberikan semangat, peringatan saat nyawa kritis, serta reaksi ketika pemain mencetak *streak* koin atau memecahkan rekor skor tertinggi (*High Score*).
- **Rintangan Arena Interaktif**: Batu rintangan di arena memiliki efek goyangan (*wobble*) ketika ditabrak saat *dash* dan menjaga jarak aman dari titik munculnya objek jatuh.
- **Penyimpanan Skor Tertinggi (*High Score*)**: Rekor skor tertinggi tersimpan secara lokal menggunakan `PlayerPrefs` dan ditampilkan pada menu utama maupun layar akhir permainan.
- **Dukungan *Input System* Ganda**: Mendukung kendali melalui papan ketik (*Keyboard* WASD / Tombol Panah) maupun kontroler (*Gamepad*).

---

## 🎮 Cara Bermain & Kontrol

| Aksi | Keyboard | Gamepad |
| :--- | :--- | :--- |
| **Bergerak (Atas / Bawah / Kiri / Kanan)** | `W` `A` `S` `D` atau `↑` `↓` `←` `→` | *Left Stick* |
| **Dash (Menghindar Cepat)** | `Left Shift` / `Right Shift` | *Button South* (`A` / `Cross`) |
| **Jeda Permainan (*Pause*)** | Tombol *Pause* pada layar | — |

### Aturan Permainan
- 🪙 **Koin (+10 Poin)**: Tangkap koin sebelum menghilang setelah mendarat di tanah. Mengumpulkan beberapa koin berturut-turut akan memicu *coin streak*.
- 💣 **Bom (-1 Nyawa)**: Hindari bom yang jatuh dari langit. Pemain memulai permainan dengan **3 nyawa**. Gunakan *dash* di saat tepat agar kebal terhadap ledakan bom.
- 🏆 **Skor Tertinggi**: Bertahanlah selama mungkin dan raih skor tertinggi untuk melampaui rekor sebelumnya!

---

## 🗂️ Struktur Proyek

```text
coin-catcher/
├── Assets/
│   ├── Audio/                  # Musik latar (BGM) dan efek suara (SFX)
│   ├── Editor/                 # Skrip utilitas kustom untuk Unity Editor
│   ├── GeneratedMeshes/        # Mesh hasil ekstraksi model 3D
│   ├── Materials/              # Material lingkungan, karakter, koin, dan bom
│   ├── Prefabs/                # Prefab koin, bom, partikel, dan komponen level
│   ├── Resources/              # Prefab rintangan (Obstacles) yang dimuat dinamis
│   ├── Scenes/                 # Scene permainan (MainMenu.unity & MainScene.unity)
│   ├── Scripts/                # Skrip logika permainan berbahasa C#
│   ├── Settings/               # Konfigurasi Universal Render Pipeline (URP)
│   ├── Sprites/                # Aset gambar 2D untuk antarmuka pengguna (UI)
│   └── ThirdParty/             # Aset 3D pihak ketiga (Kenney & Quaternius)
├── Packages/                   # Daftar dependensi paket Unity (manifest.json)
└── ProjectSettings/            # Pengaturan konfigurasi proyek Unity
```

### Daftar Skrip Utama (`Assets/Scripts/`)
| Berkas Skrip | Deskripsi Fungsi |
| :--- | :--- |
| `GameManager3D.cs` | Mengatur *state* permainan, skor, nyawa, hitung mundur, dan penyimpanan *High Score*. |
| `Player3DController.cs` | Mengelola pergerakan karakter 3D, gravitasi, batas arena, dan mekanika *dash*. |
| `PlayerAnimator.cs` | Mengatur animasi prosedural pada tubuh dan anggota gerak karakter pemain. |
| `ItemSpawner3D.cs` | Mengatur logika kemunculan koin dan bom secara berkala dengan peningkatan kesulitan. |
| `FallingItem3D.cs` | Menangani perilaku jatuh koin dan bom, deteksi tumbukan, serta pemicu ledakan. |
| `ObjectPooler3D.cs` | Mengelola *object pool* untuk koin, bom, dan efek partikel ledakan. |
| `ObstacleManager.cs` | Menghasilkan tata letak rintangan batu di arena secara acak atau berbasis *seed*. |
| `DialogManager.cs` & `DialogBubble.cs` | Mengelola pemicu dialog dan tampilan balon percakapan NPC Maya dan Pak Arka. |
| `UIManager3D.cs` & `MainMenuManager.cs` | Mengatur antarmuka menu utama, HUD skor/nyawa, indikator *dash*, dan layar *Game Over*. |
| `AudioManager.cs` | Mengelola pemutaran musik latar (BGM) dan efek suara (SFX). |
| `CameraFollow3D.cs` | Mengatur pergerakan kamera mengikuti pemain secara halus beserta efek guncangan (*shake*). |

---

## 🛠️ Teknologi & Kebutuhan Sistem

- **Game Engine**: Unity `6000.6.3f1` (Unity 6)
- **Render Pipeline**: Universal Render Pipeline (URP `17.6.0`)
- **Sistem Input**: Unity Input System (`1.20.0`)
- **UI & Tipografi**: Unity uGUI & TextMesh Pro

---

## 🚀 Cara Menjalankan Proyek di Unity Editor

1. **Kloning repositori ini**:
   ```bash
   git clone https://github.com/ikhsanbskr/coin-catcher.git
   cd coin-catcher
   ```
2. **Buka melalui Unity Hub**:
   - Buka **Unity Hub**, klik tombol **Add** -> **Add project from disk**, lalu pilih folder `coin-catcher`.
   - Pastikan menggunakan versi editor **Unity 6 (`6000.6.3f1`)** atau versi yang kompatibel.
3. **Jalankan Permainan**:
   - Buka *scene* `Assets/Scenes/MainMenu.unity` pada jendela *Project*.
   - Tekan tombol **Play (▶)** di bagian atas Unity Editor untuk mulai bermain dari menu utama.

---

## 🙏 Kredit & Atribusi Aset

- **Ultimate Nature Pack** oleh [Quaternius](https://quaternius.com/) (Lisensi CC0)
- **Starter Kit 3D Platformer** oleh [Kenney](https://kenney.nl/) (Lisensi CC0)

---

## 👨‍💻 Pengembang

Dikembangkan oleh **Muhammad Ikhsan Trianda Baskara** ([@ikhsanbskr](https://github.com/ikhsanbskr)) sebagai proyek tugas kuliah pembuatan gim.
