Platform for creating cart games
Architecture:
Unity 6 Project
│
├── Assets
│   └── Scripts
│       └── CardGame
│
├── Core
│   ├── Game
│   │   ├── GameState
│   │   ├── GamePhase
│   │   ├── GameAction
│   │   ├── GameEvent
│   │   ├── GameEngine
│   │   └── GameRules
│   │
│   ├── Cards
│   │   ├── Card
│   │   └── CardDefinition
│   │
│   ├── Players
│   └── AI
│
├── Games
│   └── SimpleCardGame
│
├── Platform
│   ├── Economy
│   │   ├── Wallet
│   │   └── EconomyService
│   │
│   ├── Storage
│   │   └── LocalPlayerData
│   │
│   └── Iap
│       ├── IIapService
│       └── Mock/Unity implementation
│
├── Presentation
│   ├── Game
│   ├── Cards
│   └── UI
│
└── Bootstrap

هدف پروژه: ساخت یک “پلتفرم 2D قابل‌استفاده مجدد برای بازی‌های کارتی نوبتی” که در آینده بتوان بازی‌های جدید را عمدتاً با اضافه‌کردن قوانین و گرافیک جدید توسعه داد.

 وضعیت فعلی تصمیمات
* فعلاً فقط Client در Unity ساخته شود.
* Online / Backend حذف شده و فعلاً بازی کاملاً Offline است.
* بازی نمونه: یک بازی کارتی ساده، بازیکن انسانی در برابر چند رقیب “AI بسیار ابتدایی”.
* هر بار شروع بازی مقداری “Coin” مصرف می‌کند.
* وقتی Coin تمام شد، کاربر باید از طریق “In-App Purchase” سکه بخرد.
* معماری باید Modular و قابل استفاده برای بازی‌های کارتی بعدی باشد.
* منطق بازی در Core از Presentation/Unity جدا باشد.
 معماری پیشنهادی 
