# PunchPal ⏱️ Time Tracking Assistant

> ⏳ A sleek, powerful, and highly customizable WPF-based time tracking tool

[![image](https://img.shields.io/github/v/release/hehang0/PunchPal.svg?label=latest)](https://github.com/HeHang0/PunchPal/releases)
[![GitHub license](https://img.shields.io/github/license/hehang0/PunchPal.svg)](https://github.com/hehang0/PunchPal/blob/master/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/hehang0/PunchPal.svg)](https://github.com/hehang0/PunchPal/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/hehang0/PunchPal.svg)](https://github.com/hehang0/PunchPal/network)
[![GitHub issues](https://img.shields.io/github/issues/hehang0/PunchPal.svg)](https://github.com/hehang0/PunchPal/issues)

[简体中文](./README.md) | English

## ✨ Key Features

- Countdown to off-work time  
- Auto-sync punch and attendance from remote sources  
- Work hour statistics and graphical overview  
- Calendar view with lunar/solar holidays  
- Highly customizable themes and window effects  
- Flexible and extensible data source integration  
- Comprehensive configuration center  

---

## 🖥 Interface Overview

![image](./image/screenshot.png)

---

## 📁 Tab Breakdown

### 📌 Records

- Monthly punch records (Time, Remark, User, Source)  
- Double-click to delete entry  

### 📌 Attendance

- View leave/overtime/travel records  
- Fields: Date, ID, Type, Start/End Time, Remark  
- Double-click to delete entry  

### 📌 Work Time

- Daily stats: clock-in/out, hours, lateness, early leave  
- Columns: Date, Start Time, End Time, Work Hours, Lateness, Early Leave, Remark  

### 📌 Calendar

- Daily blocks showing:  
  - Top-left: Date  
  - Top-right: Work hours  
  - Bottom-left: Lunar info, solar terms, holidays, or custom events  
- Bottom bar: Monthly event countdown + next holiday countdown  

### 📌 Overview

- Left: Charts showing  
  - Standard work hours  
  - Overtime on weekdays  
  - Overtime on holidays  
- Right:  
  - Monthly total hours, average per day  
  - Smart suggestions and custom alerts  

---

## ⚙️ Settings Panel

### General

- Auto start, hotkeys  
- Clock-in/out reminders (on schedule or system events)  

### Data

- Sync intervals, hour boundaries  
- Ignore early/late punches, include leaves/overtime  

### Calendar

- Week start day  
- Show lunar info, holidays, event countdowns  

### Appearance

- Theme: Light / Dark / Follow System  
- Window effect: Default / Tabbed / Mica / Acrylic  
- Background image customization (opacity, blur)  

### Work Time

- Configure flexible start time and grace period  
- Support multiple time periods (work/lunch/dinner)  

### Network

- HTTP proxy (None / System / Custom)  

### About

- Check for updates, version info, issue tracker  

---

## 🔌 Data Source System

Supports integrating with third-party or custom punch/attendance APIs.

Supported types:

- Authentication  
- User info  
- Punch records  
- Attendance records  
- Calendar info  

Request options:

- Type: GET / POST / PUT / Browser  
- Header/body supports placeholders like `{YEAR}`, `{TIMESTART}`  
- Browser mode supports cookie fetching and window automation  

Response is handled via JavaScript lambda:

```ts

interface User {
  userId: string;
  name: string;
  avatar: string;
  remark: string;
}

interface PunchRecord {
  punchTime: number; // 秒时间戳 +8h
  punchType: string;
  remark: string;
}

interface AttendanceRecord {
  attendanceId: string;
  attendanceTypeId: string;
  startTime: number;
  endTime: number;
  attendanceTime: number;
  remark: string;
}

interface CalendarRecord {
  date: number;
  festival: string;
  lunarMonth: string;
  lunarDate: string;
  lunarYear: string;
  solarTerm: string;
  isHoliday: boolean;
  isWorkday: boolean;
  remark: string;
}
```

---

## 🚀 Getting Started

``` bash
git clone https://github.com/HeHang0/PunchPal.git
cd PunchPal
# Open the solution in Visual Studio and run
```

---

## ❤️ Developer Note

"Time flows quietly, but with PunchPal, every minute of effort is remembered."

If you like this project, feel free to star, fork, report issues, or share it with friends!
