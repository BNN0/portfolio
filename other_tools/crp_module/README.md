# Stock Inventory System

A web-based stock inventory management system for manufacturing environments. This application allows users to track stock levels, manage stock movements (entries and exits), and maintain a visual timeline of stock activity.

## Features
- **Dashboard**: Visual summary of stock levels and recent activity.
- **Stock List**: Comprehensive list of all stock items with their current quantities and reserved amounts.
- **Stock Movements**: Management of stock entries (purchases/receipts) and exits (consumption/shipments).
- **Inventory Timeline**: Visual timeline of stock movements.

## Tech Stack

- **Frontend Framework**: [Vite](https://vitejs.dev/) with [React](https://reactjs.org/) (or Vanilla JS - depends on final implementation, currently set up for Vanilla JS with ES Modules).
- **Styling**: [Tailwind CSS](https://tailwindcss.com/)
- **Icons**: [Google Material Symbols](https://fonts.google.com/icons)
- **Calendar**: [FullCalendar](https://fullcalendar.io/)

## Installation & Setup

### Prerequisites
- Node.js (v14 or higher recommended)
- npm or yarn

### Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd stockinventory
   ```

2. **Install Dependencies**
   The project uses `npm` for package management.
   ```bash
   npm install
   ```

3. **Run Development Server**
   ```bash
   npm run dev
   ```
   The application will be available at `http://localhost:5173`.

4. **Build for Production**
   ```bash
   npm run build
   ```
   The production build will be created in the `dist/` directory.

## Project Structure

```
stockinventory/
├── public/                  # Static assets
│   ├── img/                 # Images
│   └── utils/               # Utility scripts (e.g., FullCalendar)
├── src/
│   ├── pages/               # Page components (index.html, inventory.html, etc.)
│   └── assets/              # Other source assets
├── index.html               # Main entry point
└── tailwind.config.js       # Tailwind CSS configuration
```

## Usage

- **Accessing Pages**: The application uses client-side routing. You can access different pages via the links in the navbar:
  - [Dashboard](/)
  - [Inventory](/inventory)

## Development

To add new pages or modify existing ones, edit the corresponding HTML files in the `src/pages/` directory.
