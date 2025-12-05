# SaveYourNote

A WhatsApp API for automatically classifying and saving messages. The API receives WhatsApp messages through webhooks, classifies them based on their content, and saves them to appropriate storage locations.

## Features

- **WhatsApp Webhook Integration**: Receives messages from WhatsApp Business API
- **Message Classification**: Automatically classifies messages into categories:
  - 🎬 **Films/Movies**: Messages mentioning movies, series, Netflix, HBO, etc.
  - 🔐 **Passwords**: Messages containing credentials or passwords
  - ✅ **Tasks**: Todo items, reminders, and action items
  - 🔗 **Links**: Messages containing URLs
  - 📝 **Notes**: General notes that don't fit other categories
  - 🎵 **Audio**: Voice messages with transcription
- **Audio Transcription**: Transcribes voice messages using OpenAI Whisper
- **Organized Storage**: Saves messages to categorized folders

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- WhatsApp Business API access
- OpenAI API key (for audio transcription)

### Configuration

Configure the following settings in `appsettings.json`:

```json
{
  "WhatsApp": {
    "AccessToken": "your-whatsapp-access-token",
    "PhoneNumberId": "your-phone-number-id",
    "VerifyToken": "your-webhook-verify-token",
    "ApiVersion": "v18.0",
    "BaseUrl": "https://graph.facebook.com"
  },
  "Storage": {
    "BasePath": "./data",
    "FilmsFolder": "films",
    "PasswordsFile": "passwords.txt",
    "TasksFolder": "tasks",
    "NotesFolder": "notes",
    "LinksFolder": "links",
    "AudioFolder": "audio"
  },
  "Transcription": {
    "Provider": "OpenAI",
    "ApiKey": "your-openai-api-key",
    "Model": "whisper-1"
  }
}
```

### Running the API

```bash
cd src/SaveYourNote.Api
dotnet run
```

The API will start on `http://localhost:5000`

### API Endpoints

#### Webhook Verification (GET)

```
GET /api/webhook?hub.mode=subscribe&hub.verify_token=your-token&hub.challenge=challenge
```

Used by WhatsApp to verify the webhook endpoint.

#### Receive Messages (POST)

```
POST /api/webhook
Content-Type: application/json

{
  "object": "whatsapp_business_account",
  "entry": [...]
}
```

Receives and processes WhatsApp messages.

#### Health Check

```
GET /health
```

Returns the API health status.

## Project Structure

```
src/
  SaveYourNote.Api/
    Controllers/        # API Controllers
    Models/             # Data Models
    Services/           # Business Logic
    Configuration/      # App Settings
tests/
  SaveYourNote.Api.Tests/  # Unit Tests
```

## Running Tests

```bash
dotnet test
```

## Storage Structure

Messages are saved in the following structure:

```
data/
  films/
    film_20231205_143022.txt
  tasks/
    task_20231205_143025.txt
  notes/
    note_20231205_143030.txt
  links/
    link_20231205_143035.txt
  audio/
    audio_msgid_20231205_143040.ogg
    audio_msgid_20231205_143040.txt  # Transcription
  passwords.txt
```

## License

MIT License - see [LICENSE](LICENSE) for details.
