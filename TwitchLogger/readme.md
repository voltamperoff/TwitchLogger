# TwitchLogger
---
**TwitchLogger** is simple application for automatic recording all chat messages and user apperance over selected channels.

The application uses SQLite database for data storage and tracks next entities:
- Channels:
  - Id (autoncremented PK)
  - Name (string) - Twitch channel name

- Users:
  - Id (autoincremented PK)
  - Name (string) - Twitch user name

- Membership:
  - Id (autoincremented PK)
  - ChannelId (FK to Channels)
  - UserId (FK to Users)

- Messages:
  - Id (autoincremented PK)
  - ChannelId (FK to Channels)
  - UserId (FK to Users)
  - Timestamp (datetime)
  - Message (string) - Twitch chat message