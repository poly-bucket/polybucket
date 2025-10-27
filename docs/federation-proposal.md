# PolyBucket Model Federation Proposal

## Overview

Model federation in PolyBucket enables multiple independent instances to share and synchronize 3D models with each other, creating a distributed network of 3D model repositories. This allows users on one instance to discover and access models from other federated instances seamlessly.

## What Model Federation Means

Model federation creates a distributed network where:

1. **Cross-Instance Model Sharing**: Users on Instance A can discover and download models from Instance B
2. **Distributed Model Library**: Creating a federated network of 3D model repositories
3. **Instance Discovery**: Automatic discovery of other PolyBucket instances
4. **Synchronized Metadata**: Keeping model information consistent across instances
5. **Access Control**: Managing permissions for federated content
6. **Content Moderation**: Handling moderation across federated instances

This is similar to how platforms like Mastodon federate social media content, but for 3D models instead of posts.

## Step-by-Step Implementation Plan

### Phase 1: Federation Handshake (Manual Process)

#### Step 1: Initial Connection Request
```
Instance A (Initiator)               Instance B (Responder)
├─ Admin initiates federation        ├─ Receives connection request
├─ Sends connection request          ├─ Checks federation settings
├─ Includes instance metadata        ├─ Verifies Instance A is legitimate
└─ Awaits response                   └─ Responds with acceptance/rejection
```

**Implementation Details:**
- Admin on Instance A enters Instance B's federation URL
- Instance A sends `POST /api/federation/handshake/initiate` to Instance B
- Request includes: Instance A's URL, name, version, and verification data
- Instance B checks if it's accepting new federations (in settings)
- Instance B responds with either:
  - `200 OK`: Open to federation (proceeds to step 2)
  - `403 Forbidden`: Not accepting new federations
  - `422 Unprocessable Entity`: Instance verification failed

#### Step 2: Cryptographic Token Exchange (Instance B → Instance A)
```
Instance A                           Instance B (Open to Federation)
├─ Receives acceptance response      ├─ Generates cryptographic token
├─ Receives Instance B's token       ├─ Sets token expiration date
├─ Stores token securely             ├─ Sends token to Instance A
└─ Token for encrypting requests     └─ Stores pending handshake record
```

**Token Properties:**
- **Format**: JWT or similar cryptographic token
- **Expiration**: Configurable (default: 90 days)
- **Renewal**: Automatic or manual renewal before expiration
- **Revocation**: Can be revoked immediately if needed
- **Scope**: Limited to federation API endpoints only

#### Step 3: Reciprocal Token Exchange (Instance A → Instance B)
```
Instance A                           Instance B
├─ Generates own cryptographic token ├─ Receives Instance A's token
├─ Sets token expiration date        ├─ Validates token format
├─ Sends token to Instance B         ├─ Stores token securely
└─ Confirms mutual authentication    └─ Activates federation connection
```

**Security Verification:**
- Both instances validate token signatures
- Tokens are stored encrypted in database
- Communication uses HTTPS only
- Both instances log the handshake event

#### Step 4: Model Catalog Exchange
```
Instance A                           Instance B
├─ Requests available models         ├─ Sends paginated model list
├─ Receives model metadata           ├─ Filters by federation settings
├─ Displays selection UI             ├─ Includes: name, description,
│                                    │   size, license, categories
└─ Admin reviews options             └─ Awaits selection

⬇️ Simultaneously ⬇️

Instance B                           Instance A
├─ Requests Instance A's models      ├─ Sends paginated model list
├─ Receives model metadata           ├─ Filters by federation settings
├─ Displays selection UI             ├─ Includes: name, description,
│                                    │   size, license, categories
└─ Admin reviews options             └─ Awaits selection
```

**Catalog Response Format:**
```json
{
  "instanceId": "instance-b-uuid",
  "instanceName": "PolyBucket Instance B",
  "totalModels": 1500,
  "page": 1,
  "pageSize": 50,
  "models": [
    {
      "id": "model-uuid",
      "name": "Model Name",
      "description": "Model description",
      "author": {
        "id": "user-uuid",
        "username": "author123",
        "displayName": "Author Name"
      },
      "filesSize": 104857600,
      "license": "CC-BY-4.0",
      "categories": ["Miniatures", "Gaming"],
      "thumbnailUrl": "https://...",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-02-20T14:45:00Z"
    }
  ]
}
```

#### Step 5: Model Selection and Import Configuration
**Implementation Decision**: Model syncing is optional; instances can federate without sharing models

```
Instance A Admin                     Instance B Admin
├─ Reviews available models          ├─ Reviews Instance A's models
├─ OPTIONAL: Select models to import ├─ OPTIONAL: Select models to import
│  • Option 1: Import All            │  • Option 1: Import All
│  • Option 2: Select Specific       │  • Option 2: Select Specific
│  • Option 3: Filter by Category    │  • Option 3: Filter by Category
│  • Option 4: Filter by License     │  • Option 4: Filter by License
│  • Option 5: None (just federate)  │  • Option 5: None (just federate)
├─ Configures sync settings:         ├─ Configures sync settings:
│  • Automatic sync: Yes/No          │  • Automatic sync: Yes/No
│  • Sync interval: Daily/Weekly     │  • Sync interval: Daily/Weekly
│  • Auto-import new models: Yes/No  │  • Auto-import new models: Yes/No
│  • Batch size: 100 (configurable)  │  • Batch size: 100 (configurable)
│  • Batch interval: 20 min (config) │  • Batch interval: 20 min (config)
│  • Rate limit: Admin-configurable  │  • Rate limit: Admin-configurable
└─ Initiates import process (optional)└─ Initiates import process (optional)
```

**Key Point**: Federation can exist without model import
- Instances can establish federation for future use
- Model syncing can be enabled/disabled independently
- Each instance decides what to import (one-way or bidirectional)
- "Federated but not syncing" is a valid state

**Sync Configuration Options:**
- **Manual Sync Only**: Admin manually selects and imports models
- **Automatic New Models**: Auto-import new models matching filters
- **Automatic Updates**: Auto-sync updates to imported models
- **Scheduled Sync**: Daily/Weekly/Monthly sync schedule
- **Bandwidth Limits**: Max data transfer per day/week
- **Category Filters**: Only sync specific categories
- **License Filters**: Only sync models with specific licenses
- **Size Limits**: Skip models over X MB

#### Step 6: Model Import and Local Copy Creation
```
Instance A                           Instance B
├─ Sends import request              ├─ Validates Instance A's token
├─ Receives file URLs                ├─ Generates presigned URLs
├─ Downloads model files             ├─ Tracks download progress
├─ Creates federated user records    ├─ Updates sync statistics
├─ Creates federated model records   ├─ Logs federation activity
└─ Updates sync status               └─ Completes handshake
```

**Import Process:**
1. Instance A sends list of selected model IDs
2. Instance B validates request and generates presigned URLs for files
3. Instance A downloads all files for each model
4. Instance A creates/updates federated User records for authors
5. Instance A creates local Model records with federation metadata
6. Both instances mark the federation as "Active"
7. Federation handshake is complete

**Handshake Completion:**
- Both instances have exchanged tokens
- Both instances have selected models (or none)
- Import process is initiated (may continue in background)
- Federation status changes to "Active"
- Admins receive confirmation notification

### Phase 2: Federation Settings and Configuration

#### Federation Global Settings
```
System Settings → Federation
├─ Accept New Federations: Yes/No
├─ Require Admin Approval: Yes/No
├─ Auto-approve Known Instances: Yes/No
├─ Default Token Expiration: 90 days
├─ Instance Verification Required: Yes/No
├─ Max Federation Connections: 50
├─ Default Sync Settings:
│  ├─ Allow Automatic Sync: Yes/No
│  ├─ Default Sync Interval: Weekly
│  ├─ Max Bandwidth per Day: 10GB
│  └─ Auto-import New Models: No
└─ Content Filters:
   ├─ Allowed Licenses: All/Specific
   ├─ Blocked Categories: None/Specific
   └─ Max Model Size: 500MB
```

#### Per-Instance Federation Settings
```
Federation Instance Configuration
├─ Instance Name: "PolyBucket Instance B"
├─ Base URL: "https://instance-b.com"
├─ Status: Active/Inactive/Pending/Blocked
├─ Token Status:
│  ├─ Our Token: Valid (expires: 2025-04-15)
│  └─ Their Token: Valid (expires: 2025-04-20)
├─ Sync Configuration:
│  ├─ Sync Mode: Manual/Automatic
│  ├─ Sync Interval: Daily/Weekly/Monthly
│  ├─ Last Sync: 2025-01-15 14:30 UTC
│  ├─ Next Sync: 2025-01-22 14:30 UTC
│  └─ Auto-import New Models: Yes/No
├─ Import Filters:
│  ├─ Categories: All/Selected
│  ├─ Licenses: All/Selected
│  ├─ Max File Size: 500MB
│  └─ Import All New Models: No
├─ Statistics:
│  ├─ Models Imported: 150
│  ├─ Models Shared: 75
│  ├─ Data Transferred: 25.3 GB
│  └─ Last Activity: 2025-01-15 14:30 UTC
└─ Actions:
   ├─ Sync Now (Manual)
   ├─ Refresh Token
   ├─ Revoke Federation
   └─ View Sync Logs
```

### Phase 3: Ongoing Synchronization (Automatic/Manual)

#### Automatic Sync Process (If Enabled)
```
Instance A (Scheduled Sync)          Instance B
├─ Timer triggers sync check         ├─ Receives sync request
├─ Authenticates with token          ├─ Validates token
├─ Requests models since last sync   ├─ Returns new/updated models
├─ Filters by instance settings      ├─ Filters by permissions
├─ Downloads new models              ├─ Generates presigned URLs
├─ Updates existing models           ├─ Tracks bandwidth usage
└─ Logs sync activity                └─ Updates statistics
```

**Sync Triggers:**
- **Scheduled**: Based on configured interval (Daily/Weekly/Monthly)
- **Manual**: Admin clicks "Sync Now" button
- **Event-driven** (Future): Webhook notification from remote instance

#### Token Renewal Process
**Implementation Decision**: Automatic, timed, or manual renewal via handshake refresh

```
Instance A (Token Expiring)          Instance B
├─ Detects token expires in 7 days   ├─ Receives renewal request
├─ Option 1: Automatic Renewal       ├─ Option 1: Auto-accept renewal
│  └─ System auto-sends renewal      │  └─ System auto-generates new token
├─ Option 2: Timed Renewal           ├─ Option 2: Scheduled acceptance
│  └─ Renewal at specific time       │  └─ Accepts during renewal window
├─ Option 3: Manual Renewal          ├─ Option 3: Manual approval
│  ├─ Admin clicks "Refresh Token"   │  ├─ Admin receives notification
│  └─ Sends renewal request          │  ├─ Admin clicks "Approve Renewal"
│                                    │  └─ Generates new token
├─ Authenticates with current token  ├─ Validates current token
├─ Receives new token                ├─ Sends new token
├─ Stores new token                  ├─ Stores new token
└─ Updates token expiry date         └─ Logs renewal event
```

**Token Renewal Options:**
1. **Automatic Renewal** (Default)
   - Auto-renew 7 days before expiration
   - No admin intervention needed
   - Both instances must have auto-renewal enabled
   - Falls back to manual if either side rejects

2. **Timed Renewal**
   - Renewal at specific scheduled time (e.g., first day of month)
   - Admin configures renewal schedule
   - Notification sent before renewal
   - Can set renewal window (e.g., days 1-7 of month)

3. **Manual Renewal**
   - Admin clicks "Refresh Handshake" button
   - Sends renewal request to remote instance
   - Remote admin receives notification
   - Remote admin approves or rejects
   - If approved, full handshake process repeats
   - Syncing resumes where it left off

**Re-Handshake After Expiration:**
```
Token Expired Scenario:
Instance A                           Instance B
├─ Token has expired                 ├─ Token has expired
├─ Sync jobs fail (token invalid)    ├─ Sync jobs fail (token invalid)
├─ Status: "Token Expired"           ├─ Status: "Token Expired"
├─ Admin sees "Refresh Handshake"    ├─ Admin sees "Refresh Handshake"
├─ Option 1: Instance A initiates    ├─ Option 1: Instance B initiates
│  ├─ Clicks "Refresh Handshake"     │  ├─ Receives renewal request
│  ├─ Pings Instance B               │  ├─ Admin clicks "Accept"
│  └─ Awaits response                │  └─ Handshake begins again
├─ Option 2: Instance B initiates    ├─ Option 2: Instance A responds
│  ├─ Receives renewal request       │  ├─ Clicks "Refresh Handshake"
│  ├─ Admin clicks "Accept"          │  ├─ Pings Instance A
│  └─ Handshake begins again         │  └─ Awaits response
└─ Full handshake process repeats    └─ Full handshake process repeats
   (Steps 1-6 from initial handshake)   (Steps 1-6 from initial handshake)
```

**Key Points:**
- Either instance can initiate renewal
- Renewal requires approval from both sides (for manual mode)
- Automatic renewal happens silently
- Syncing resumes from last successful sync point
- Import queue is preserved during renewal
- No data loss during token expiration

**Notification Schedule:**
- 30 days before expiry: Info notification
- 14 days before expiry: Warning notification
- 7 days before expiry: Urgent notification + auto-renewal trigger
- 1 day before expiry: Critical notification
- Expired: Sync stops, admin intervention required (if manual mode)

#### Federation Revocation Process
**Implementation Decision**: Models remain, syncing stops, soft delete federation records

```
Instance A (Revoking)                Instance B
├─ Admin clicks "Revoke Federation"  ├─ Receives revocation notice (optional)
├─ Soft deletes federation record    ├─ Soft deletes federation record
├─ Nullifies token agreement         ├─ Nullifies token agreement
├─ All imported models remain        ├─ All imported models remain
├─ Syncing stops immediately         ├─ Syncing stops immediately
├─ Federation status: "Revoked"      ├─ Federation status: "Revoked"
└─ Logs revocation event             └─ Logs revocation event
```

**Key Points:**
- **Models Persist**: All imported models stay on both instances
- **No Data Loss**: Users retain access to federated content
- **Soft Delete**: Federation records marked as deleted, not removed
- **Sync Stops**: No further synchronization occurs
- **Attribution Remains**: Federated model metadata stays intact
- **Re-federation Possible**: Can re-establish federation later

**Revocation Flow:**
1. Admin clicks "Revoke Federation"
2. Confirmation dialog: "Models will remain but syncing stops. Continue?"
3. System soft-deletes federation record (IsDeleted = true)
4. Optional: Attempt to notify remote instance (best effort, not required)
5. Stop all scheduled sync jobs for this instance
6. Update status to "Revoked"

**Revocation Reasons:**
- Policy violations
- Security concerns
- Instance shutdown
- Manual admin decision
- Token compromise
- No longer needed

### Phase 4: Permission and Access Control

#### Model Filtering During Sync
```
Instance A (Requesting)              Instance B (Responding)
├─ Requests models catalog           ├─ Applies federation filters:
├─ Includes filter preferences:      │  ├─ Check model privacy (public only)
│  ├─ Categories: Miniatures, Props  │  ├─ Check license compatibility
│  ├─ Licenses: CC-BY, CC-BY-SA     │  ├─ Check file size limits
│  └─ Max Size: 500MB                │  ├─ Check category allowlist
├─ Receives filtered catalog         │  └─ Check instance permissions
└─ Displays available models         └─ Returns filtered catalog
```

**Permission Hierarchy:**
1. **Global Federation Settings**: Instance-wide rules
2. **Per-Instance Settings**: Specific rules for this federation
3. **Model Settings**: Individual model privacy/federation settings
4. **License Compatibility**: Auto-filter incompatible licenses

### Phase 3: Content Synchronization

#### Step 5: File Transfer Process
```
Instance A (Source)                  Instance B (Target)
├─ Receives sync request             ├─ Initiates model download
├─ Validates permissions             ├─ Requests file URLs
├─ Generates presigned URLs         ├─ Downloads files
├─ Transfers model files             ├─ Validates file integrity
└─ Updates sync status              └─ Stores files locally
```

**File Transfer Flow:**
1. Instance B requests model files from Instance A
2. Instance A generates presigned URLs for file access
3. Instance B downloads files directly from Instance A's storage
4. Files are stored in Instance B's storage system
5. Metadata is updated to reflect local file locations

#### Step 6: Thumbnail and Preview Generation
```
Instance A                           Instance B
├─ Provides thumbnail URLs          ├─ Downloads thumbnails
├─ Shares preview generation         ├─ Generates local previews
├─ Exports model metadata           ├─ Updates local metadata
└─ Maintains original files        └─ Creates local references
```

### Phase 4: Ongoing Synchronization

#### Step 7: Change Detection and Updates
```
Instance A (Publisher)               Instance B (Subscriber)
├─ Model updated/deleted             ├─ Polls for changes
├─ Updates federation metadata       ├─ Detects changes
├─ Notifies federated instances      ├─ Downloads updates
└─ Maintains change log              └─ Applies changes locally
```

**Sync Mechanisms:**
- **Polling**: Instance B periodically checks for updates
- **Webhooks**: Instance A notifies Instance B of changes
- **Event-driven**: Real-time updates via WebSocket connections

#### Step 8: Conflict Resolution
```
Instance A                           Instance B
├─ Model deleted                     ├─ Model still referenced
├─ Sends deletion notification       ├─ Handles orphaned references
├─ Provides grace period             ├─ Updates local metadata
└─ Removes from federation           └─ Maintains local copy
```

### Phase 5: User Experience

#### Step 9: Transparent Access
```
User on Instance B                   Instance A (Remote)
├─ Browses models                    ├─ Provides model metadata
├─ Sees federated content            ├─ Handles file requests
├─ Downloads seamlessly              ├─ Tracks usage statistics
└─ Experiences local performance     └─ Maintains original files
```

**User Experience Features:**
- Federated models appear in local search results
- Seamless download experience
- Clear indication of federated content
- Local caching for performance

#### Step 10: Analytics and Monitoring
```
Instance A                           Instance B
├─ Tracks federation statistics      ├─ Reports usage to Instance A
├─ Monitors sync health              ├─ Provides download metrics
├─ Handles error recovery            ├─ Manages local storage
└─ Maintains audit logs             └─ Updates sync status
```

## Technical Implementation

### Database Schema Additions

```sql
-- Federation instances (updated with token management)
CREATE TABLE FederatedInstances (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    BaseUrl NVARCHAR(500) NOT NULL,
    Status NVARCHAR(50) NOT NULL,  -- Pending, Active, Inactive, Blocked, Error
    
    -- Token Management
    OurToken NVARCHAR(MAX) NULL,              -- Token we send to them (encrypted)
    OurTokenExpiry DATETIME2 NULL,             -- When our token expires
    TheirToken NVARCHAR(MAX) NULL,             -- Token they send to us (encrypted)
    TheirTokenExpiry DATETIME2 NULL,           -- When their token expires
    TokenRenewalMode NVARCHAR(50) NOT NULL DEFAULT 'Automatic',  -- Automatic/Manual
    
    -- Sync Configuration
    SyncMode NVARCHAR(50) NOT NULL DEFAULT 'Manual',  -- Manual/Automatic
    SyncInterval NVARCHAR(50) NULL,            -- Daily/Weekly/Monthly
    AutoImportNewModels BIT NOT NULL DEFAULT 0,
    LastSyncAt DATETIME2 NULL,
    NextSyncAt DATETIME2 NULL,
    
    -- Import Filters
    AllowedCategories NVARCHAR(MAX) NULL,      -- JSON array of category IDs
    AllowedLicenses NVARCHAR(MAX) NULL,        -- JSON array of license types
    MaxModelSizeMB INT NOT NULL DEFAULT 500,
    MaxBandwidthPerDayGB DECIMAL(10,2) NULL,
    
    -- Statistics
    ModelsImported INT NOT NULL DEFAULT 0,
    ModelsShared INT NOT NULL DEFAULT 0,
    TotalBytesTransferred BIGINT NOT NULL DEFAULT 0,
    LastActivityAt DATETIME2 NULL,
    
    -- Audit fields
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL
);

-- Federation handshake tracking
CREATE TABLE FederationHandshakes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InitiatorInstanceId UNIQUEIDENTIFIER NULL,       -- NULL if we initiated
    ResponderInstanceId UNIQUEIDENTIFIER NULL,       -- NULL if they initiated
    InitiatorUrl NVARCHAR(500) NOT NULL,
    ResponderUrl NVARCHAR(500) NOT NULL,
    Status NVARCHAR(50) NOT NULL,              -- Initiated, TokenExchanged, Completed, Rejected, Failed
    HandshakeToken NVARCHAR(MAX) NULL,         -- Temporary token for handshake
    ErrorMessage NVARCHAR(MAX) NULL,
    CompletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    ExpiresAt DATETIME2 NOT NULL
);

-- Federation sync logs (enhanced)
CREATE TABLE FederationSyncLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InstanceId UNIQUEIDENTIFIER NOT NULL,
    SyncType NVARCHAR(50) NOT NULL,            -- Manual, Automatic, TokenRenewal, Handshake
    Action NVARCHAR(50) NOT NULL,              -- Sync, Import, Export, Renew, Revoke
    Status NVARCHAR(50) NOT NULL,              -- Started, InProgress, Completed, Failed
    ModelsProcessed INT NOT NULL DEFAULT 0,
    BytesTransferred BIGINT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(MAX) NULL,
    ErrorDetails NVARCHAR(MAX) NULL,           -- JSON error details
    StartedAt DATETIME2 NOT NULL,
    CompletedAt DATETIME2 NULL,
    DurationMs INT NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    FOREIGN KEY (InstanceId) REFERENCES FederatedInstances(Id)
);

-- Federation model import queue (for tracking import progress)
CREATE TABLE FederationImportQueue (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InstanceId UNIQUEIDENTIFIER NOT NULL,
    RemoteModelId UNIQUEIDENTIFIER NOT NULL,
    LocalModelId UNIQUEIDENTIFIER NULL,        -- NULL until imported
    Status NVARCHAR(50) NOT NULL,              -- Pending, Downloading, Processing, Completed, Failed
    Priority INT NOT NULL DEFAULT 0,           -- Higher priority = import first
    FilesDownloaded INT NOT NULL DEFAULT 0,
    TotalFiles INT NOT NULL DEFAULT 0,
    BytesDownloaded BIGINT NOT NULL DEFAULT 0,
    TotalBytes BIGINT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    FOREIGN KEY (InstanceId) REFERENCES FederatedInstances(Id)
);

-- Federation global settings
CREATE TABLE FederationSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AcceptNewFederations BIT NOT NULL DEFAULT 1,
    RequireAdminApproval BIT NOT NULL DEFAULT 1,
    AutoApproveKnownInstances BIT NOT NULL DEFAULT 0,
    DefaultTokenExpirationDays INT NOT NULL DEFAULT 90,
    InstanceVerificationRequired BIT NOT NULL DEFAULT 1,
    MaxFederationConnections INT NOT NULL DEFAULT 50,
    AllowAutomaticSync BIT NOT NULL DEFAULT 1,
    DefaultSyncInterval NVARCHAR(50) NOT NULL DEFAULT 'Weekly',
    MaxBandwidthPerDayGB DECIMAL(10,2) NOT NULL DEFAULT 10.0,
    AutoImportNewModels BIT NOT NULL DEFAULT 0,
    AllowedLicenses NVARCHAR(MAX) NULL,        -- JSON array
    BlockedCategories NVARCHAR(MAX) NULL,      -- JSON array
    MaxModelSizeMB INT NOT NULL DEFAULT 500,
    UpdatedAt DATETIME2 NOT NULL,
    UpdatedBy UNIQUEIDENTIFIER NOT NULL
);

-- Audit log for federation events
CREATE TABLE FederationAuditLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InstanceId UNIQUEIDENTIFIER NULL,
    EventType NVARCHAR(100) NOT NULL,          -- HandshakeInitiated, TokenExchanged, FederationRevoked, etc.
    EventData NVARCHAR(MAX) NULL,              -- JSON event details
    UserId UNIQUEIDENTIFIER NULL,              -- Admin who triggered the event
    IpAddress NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (InstanceId) REFERENCES FederatedInstances(Id)
);
```

### API Endpoints Structure

```
/api/federation/
├── handshake/
│   ├── POST /initiate                    # Initiate federation handshake
│   ├── POST /accept                      # Accept federation request
│   ├── POST /exchange-token              # Exchange cryptographic tokens
│   ├── POST /complete                    # Complete handshake
│   └── POST /reject                      # Reject federation request
├── instances/
│   ├── GET /                             # List federated instances
│   ├── GET /{id}                         # Get specific instance
│   ├── POST /                            # Manually add instance (starts handshake)
│   ├── PUT /{id}                         # Update instance settings
│   ├── DELETE /{id}                      # Revoke federation
│   ├── POST /{id}/sync                   # Trigger manual sync
│   ├── POST /{id}/refresh-token          # Refresh cryptographic token
│   └── GET /{id}/statistics              # Get federation statistics
├── models/
│   ├── GET /                             # List available models for federation
│   ├── GET /{id}                         # Get specific model details
│   ├── POST /import                      # Import selected models
│   ├── GET /imported                     # List locally imported models
│   └── DELETE /imported/{id}             # Remove imported model
├── catalog/
│   ├── GET /                             # Get paginated catalog (for remote instances)
│   ├── GET /changes                      # Get models changed since timestamp
│   └── GET /statistics                   # Get catalog statistics
├── sync/
│   ├── GET /status                       # Get overall sync status
│   ├── GET /status/{instanceId}          # Get instance-specific sync status
│   ├── POST /start/{instanceId}          # Start sync for specific instance
│   ├── POST /stop/{instanceId}           # Stop ongoing sync
│   └── GET /logs                         # Get sync logs
├── settings/
│   ├── GET /global                       # Get global federation settings
│   ├── PUT /global                       # Update global federation settings
│   ├── GET /{instanceId}                 # Get instance-specific settings
│   └── PUT /{instanceId}                 # Update instance-specific settings
├── tokens/
│   ├── GET /{instanceId}/status          # Check token status
│   ├── POST /{instanceId}/renew          # Renew token
│   └── POST /{instanceId}/revoke         # Revoke token
└── health/
    ├── GET /                             # Check overall federation health
    ├── GET /{instanceId}                 # Check specific instance health
    └── GET /{instanceId}/connectivity    # Test connectivity to instance
```

### Security Considerations

1. **Authentication**: Mutual TLS or API key authentication
2. **Authorization**: Role-based access control for federation
3. **Data Integrity**: File checksums and signature verification
4. **Privacy**: Respect for model privacy settings
5. **Rate Limiting**: Prevent abuse of federation endpoints

### Performance Optimizations

1. **Incremental Sync**: Only sync changed models
2. **Compression**: Compress files during transfer
3. **Caching**: Cache frequently accessed federated content
4. **CDN Integration**: Use CDN for file distribution
5. **Background Processing**: Async sync operations

## Key Questions and Considerations

### Technical Challenges

1. **File Storage**: How to handle large 3D model files across instances?
2. **Bandwidth**: Managing bandwidth usage for file transfers
3. **Storage Costs**: Who pays for storage of federated content?
4. **Version Control**: How to handle model updates and versioning?
5. **Conflict Resolution**: What happens when models are modified on both sides?

### User Experience Questions

1. **Discovery**: How do users discover federated content?
2. **Attribution**: How to properly attribute federated models?
3. **Licensing**: How to handle different licensing terms?
4. **Moderation**: How to handle content moderation across instances?
5. **Performance**: How to ensure good performance for federated content?

### Business and Legal Considerations

1. **Cost Sharing**: How to distribute costs of federation?
2. **Liability**: Who is responsible for federated content?
3. **Compliance**: How to handle different legal requirements?
4. **Quality Control**: How to maintain content quality across instances?
5. **Dispute Resolution**: How to handle conflicts between instances?

## Low-Hanging Fruits Implementation Plan

Based on the existing PolyBucket architecture, here are the **low-hanging fruits** for federation that require minimal refactoring:

### **1. Federation Metadata Extension (Easiest)**
**Effort**: 2-3 days | **Risk**: Very Low

**What**: Add federation fields to existing models and users without breaking changes.

**IMPORTANT DESIGN DECISION**: Federated models and users create LOCAL COPIES with full redundancy. 
When importing a model from a federated instance:
- All model files are downloaded and stored locally
- A local User record is created for the remote author (marked as federated)
- A local Model record is created (marked as federated)
- Metadata tracks the original source instance and user
- Collections remain per-instance (no federation)

```csharp
// Add to existing Model.cs
public class Model : Auditable
{
    // ... existing properties ...
    
    // Federation fields (nullable for backward compatibility)
    public string? RemoteInstanceId { get; set; }     // Origin instance URL/ID
    public string? RemoteModelId { get; set; }         // Original model ID on source instance
    public Guid? RemoteAuthorId { get; set; }          // Links to local federated User copy
    public bool IsFederated { get; set; } = false;     // Is this a federated copy?
    public DateTime? LastFederationSync { get; set; }  // Last sync with origin
}

// Add to existing User.cs
public class User : Auditable
{
    // ... existing properties ...
    
    // Federation fields (nullable for backward compatibility)
    public string? RemoteInstanceId { get; set; }     // Origin instance if federated user
    public string? RemoteUserId { get; set; }          // Original user ID on source instance
    public bool IsFederated { get; set; } = false;     // Is this a federated user copy?
    public bool CanLogin { get; set; } = true;         // Federated users cannot login
    public DateTime? LastFederationSync { get; set; }  // Last sync with origin
}
```

**Why it's easy**: 
- No breaking changes to existing code
- Frontend already handles optional fields gracefully
- Database migration is straightforward
- Full redundancy means no complex remote file access needed

### **2. Federation Instance Management (Easy)**
**Effort**: 3-4 days | **Risk**: Low

**What**: Create a simple federation instances table and basic CRUD.

```csharp
// New entity - minimal impact
public class FederatedInstance : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Active, Inactive
    public DateTime? LastSyncAt { get; set; }
}
```

**Why it's easy**:
- Follows existing patterns (Auditable, similar to User entity)
- Can use existing API patterns from SystemSettings
- No complex business logic required

### **3. Basic Federation API Endpoints (Easy)**
**Effort**: 4-5 days | **Risk**: Low

**What**: Create federation endpoints following existing patterns.

```csharp
// Follow existing controller patterns
[ApiController]
[Route("api/federation")]
[Authorize] // Use existing auth
public class FederationController : ControllerBase
{
    [HttpGet("instances")]
    public async Task<ActionResult<List<FederatedInstance>>> GetInstances()
    
    [HttpPost("instances")]
    public async Task<ActionResult<FederatedInstance>> AddInstance()
    
    [HttpGet("models")]
    public async Task<ActionResult<List<Model>>> GetFederatedModels()
}
```

**Why it's easy**:
- Copy patterns from existing controllers (SystemSettings, Models)
- Use existing authentication and authorization
- Follow established CQRS patterns

### **4. Frontend Federation Indicators (Easy)**
**Effort**: 2-3 days | **Risk**: Very Low

**What**: Add visual indicators for federated content in existing components.

```typescript
// Extend existing ModelCard component
const ModelCard = ({ model }: { model: ExtendedModel }) => {
  return (
    <div className="model-card">
      {/* Existing content */}
      
      {/* Add federation indicator */}
      {model.isFederated && (
        <div className="federation-badge">
          <GlobeIcon />
          <span>@{model.remoteAuthorUsername}@{model.remoteInstanceName}</span>
        </div>
      )}
    </div>
  );
};
```

**Why it's easy**:
- Frontend already handles conditional rendering
- ModelCard component is well-structured
- No changes to existing functionality

### **5. Federation Model Import Process (Medium)**
**Effort**: 5-7 days | **Risk**: Medium

**What**: Implement the full import process for federated models.

```csharp
// Federation import service
public class FederationImportService
{
    public async Task<Model> ImportModelAsync(string instanceId, string remoteModelId)
    {
        // 1. Fetch model metadata from remote instance
        var remoteModel = await FetchRemoteModelMetadata(instanceId, remoteModelId);
        
        // 2. Create or get local federated user (author)
        var localAuthor = await GetOrCreateFederatedUser(remoteModel.Author, instanceId);
        
        // 3. Download all model files to local storage
        var localFiles = await DownloadModelFiles(remoteModel.Files, instanceId);
        
        // 4. Create local model record with federation metadata
        var localModel = new Model
        {
            // Copy all model properties
            Name = remoteModel.Name,
            Description = remoteModel.Description,
            // ... other properties ...
            
            // Set federation metadata
            IsFederated = true,
            RemoteInstanceId = instanceId,
            RemoteModelId = remoteModelId,
            RemoteAuthorId = localAuthor.Id,
            AuthorId = localAuthor.Id,
            LastFederationSync = DateTime.UtcNow
        };
        
        // 5. Save to local database
        await _dbContext.Models.AddAsync(localModel);
        await _dbContext.SaveChangesAsync();
        
        return localModel;
    }
    
    private async Task<User> GetOrCreateFederatedUser(RemoteUser remoteUser, string instanceId)
    {
        // Check if federated user already exists
        var existing = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RemoteUserId == remoteUser.Id && 
                                     u.RemoteInstanceId == instanceId);
        
        if (existing != null)
            return existing;
        
        // Create new federated user
        var federatedUser = new User
        {
            Username = $"{remoteUser.Username}@{instanceId}",
            DisplayName = remoteUser.DisplayName,
            Email = $"federated-{remoteUser.Id}@{instanceId}",
            AvatarUrl = remoteUser.AvatarUrl,
            IsFederated = true,
            CanLogin = false,  // Federated users cannot login
            RemoteInstanceId = instanceId,
            RemoteUserId = remoteUser.Id,
            LastFederationSync = DateTime.UtcNow
        };
        
        await _dbContext.Users.AddAsync(federatedUser);
        await _dbContext.SaveChangesAsync();
        
        return federatedUser;
    }
}
```

**Why it's manageable**:
- Follows existing file upload patterns
- Uses existing storage infrastructure
- Clear step-by-step process
- Full local copies simplify access patterns

### **6. Federation Search Integration (Easy)**
**Effort**: 3-4 days | **Risk**: Low

**What**: Federated models automatically appear in search (they're local copies).

**Why it's easy**:
- Federated models are stored locally in the Models table
- Existing search already works with them (no changes needed!)
- Just add filter option: "Show only federated" or "Hide federated"
- Frontend can add badges to distinguish federated results

## 🚫 **What to Avoid (High Effort)**

### **Complex Features to Skip Initially**
1. **Real-time synchronization** - Requires complex event systems
2. **Cross-instance user authentication** - Major security implications  
3. **Federated collections** - Complex data relationships
4. **Automatic file synchronization** - Storage and bandwidth complexity
5. **Cross-instance social features** - Complex user identity management

## 📋 **Implementation Order**

### **Phase 1: Foundation (Week 1)**
1. Add federation fields to Model entity
2. Create FederatedInstance entity
3. Basic federation instance management API

### **Phase 2: Display (Week 2)**  
1. Frontend federation indicators
2. Federation metadata display
3. Basic federation model listing

### **Phase 3: Discovery (Week 3)**
1. Federation search integration
2. Manual federation model import
3. Basic federation health checks

## 🎯 **Success Criteria**

**MVP Federation should enable**:
- ✅ Admins can add and manage federation instances
- ✅ Manual import of models from federated instances
- ✅ Full local copies of models with complete file redundancy
- ✅ Automatic creation of federated user records for proper attribution
- ✅ Clear visual distinction of federated content
- ✅ Federated models appear in local search (as local copies)
- ✅ Metadata tracks original source instance and author
- ✅ Federated users cannot login (read-only attribution)

**What it WON'T do initially**:
- ❌ Automatic synchronization (manual import only)
- ❌ Cross-instance user authentication
- ❌ Federated collections (collections remain per-instance)
- ❌ Real-time updates from source instances
- ❌ Bi-directional sync (no push back to source)

**Key Benefits of Local Copy Approach**:
- 🚀 **Performance**: All file access is local (no remote calls)
- 💪 **Resilience**: Models remain accessible if source instance goes offline
- 🔒 **Independence**: Each instance operates fully independently
- 📊 **Storage Redundancy**: Natural backup across federated network
- 🎯 **Simplicity**: No complex remote file access or caching needed
- 🏷️ **Attribution**: Full tracking of origin instance and author

This approach gives you a working federation system in 2-3 weeks with minimal risk and maximum learning value. Once this foundation is solid, you can incrementally add more complex features.

## Handshake Process Review & Gaps Analysis

### ✅ What You've Covered Well

Your proposed handshake process is comprehensive and well-thought-out:

1. **Initial Connection & Verification**
   - ✅ Instance A initiates connection
   - ✅ Instance B can accept or reject
   - ✅ Response indicates openness to federation

2. **Cryptographic Token Exchange**
   - ✅ Bidirectional token exchange (A ↔ B)
   - ✅ Token expiration with renewal
   - ✅ Secure storage of tokens
   - ✅ Tokens used for encrypted communication

3. **Model Catalog Exchange**
   - ✅ Bidirectional catalog sharing
   - ✅ Paginated model lists
   - ✅ Metadata included in catalog

4. **Model Selection & Import**
   - ✅ Admin manual selection option
   - ✅ "Import All" option
   - ✅ Filter by categories/licenses
   - ✅ Local copy creation with user records

5. **Automation Configuration**
   - ✅ Manual handshake process
   - ✅ Automated sync after handshake
   - ✅ Clear distinction between manual and automatic

### 🔍 Additional Considerations & Recommendations

Here are important aspects to consider for a robust implementation:

#### 1. **Instance Verification & Trust**
**Implementation Decision**: Primarily through cryptographic keys/tokens

**Approach:**
- **Primary**: Cryptographic token exchange (JWT with RS256)
- **Secondary**: Instance metadata (URL, name, version) for informational purposes only
- **Trust Model**: Token possession = legitimate instance

```
Verification Process:
Instance A → Instance B
├─ Sends: Instance URL, Name, Version
├─ Instance B validates:
│  ├─ Is HTTPS? (required)
│  ├─ Can reach /api/federation/health? (basic connectivity)
│  └─ Admin decision: Accept or Reject
├─ If accepted:
│  └─ Token exchange establishes trust
└─ Token possession = verified instance
```

**Why This Works:**
- Cryptographic tokens are self-validating
- Token exchange proves bidirectional connectivity
- Eliminates need for complex verification infrastructure
- Admin approval adds human oversight layer

**Optional Enhancements (Future):**
- Version compatibility checks (warn if major version mismatch)
- Instance reputation tracking
- Community blocklist integration

#### 2. **Token Security & Storage**
**Gap**: How are tokens generated, stored, and secured?

**Recommendations:**
- **Token Format**: Use JWT with RS256 (asymmetric encryption)
- **Token Storage**: Encrypt tokens at rest in database
- **Token Scope**: Limit tokens to specific federation endpoints only
- **Token Rotation**: Implement automatic rotation before expiry
- **Revocation List**: Maintain a list of revoked tokens

```csharp
// Token structure
{
  "iss": "instance-a-id",          // Issuer (who created token)
  "aud": "instance-b-id",          // Audience (who can use token)
  "sub": "federation",             // Subject
  "iat": 1234567890,               // Issued at
  "exp": 1234567890,               // Expires at
  "scope": "federation:read,federation:write",
  "instance_url": "https://instance-a.com",
  "instance_name": "PolyBucket Instance A"
}
```

#### 3. **Handshake State Management**
**Gap**: What happens if handshake is interrupted?

**Recommendations:**
- **Handshake Timeout**: Handshake expires after 24 hours if not completed
- **Resume Capability**: Allow resuming from last completed step
- **State Persistence**: Store handshake state in `FederationHandshakes` table
- **Cleanup**: Auto-delete expired incomplete handshakes

```
Handshake States:
1. Initiated        → Connection request sent
2. Accepted         → Instance B accepted, sent token
3. TokenExchanged   → Both tokens exchanged
4. CatalogShared    → Both catalogs shared
5. ModelsSelected   → Admin selected models
6. Completed        → Handshake complete, federation active
7. Rejected         → Instance rejected connection
8. Expired          → Handshake timed out
9. Failed           → Error occurred during handshake
```

#### 4. **Bandwidth & Rate Limiting**
**Implementation Decision**: Admin-configurable rate limiting with Hangfire batch jobs

**Approach:**
- **Batch Processing**: Import 100 models every 20 minutes (default, configurable)
- **Job Scheduler**: Use Hangfire for scheduled sync jobs
- **Rate Limiting**: Admin-configurable per instance
- **Queue Management**: Process imports in controlled batches
- **Bandwidth Tracking**: Monitor and enforce daily/weekly limits

```
Sync Job Configuration (Hangfire):
├─ Batch Size: 100 models (admin-configurable)
├─ Batch Interval: 20 minutes (admin-configurable)
├─ Max Concurrent Jobs: 1 per instance
├─ Job Priority: Normal (can be elevated)
└─ Retry Policy: 3 attempts with exponential backoff

Rate Limits (Admin-Configurable):
├─ Handshake: 5 attempts per hour per IP (system default)
├─ Catalog Requests: 100 requests per hour (per instance)
├─ Model Import Batch Size: 100 models (configurable: 10-1000)
├─ Batch Interval: 20 minutes (configurable: 5-1440 min)
├─ Bandwidth: Admin-configurable (default 10GB/day)
└─ Concurrent Downloads: 5 files at once (per batch)

Hangfire Job Types:
├─ FederationSyncJob
│  ├─ Checks for new/updated models
│  ├─ Queues models for import
│  └─ Runs on configured interval
├─ FederationImportJob
│  ├─ Processes import queue in batches
│  ├─ Downloads model files
│  └─ Creates local copies
├─ FederationHealthCheckJob
│  ├─ Pings federated instances
│  ├─ Checks token expiry
│  └─ Updates connection status
└─ FederationTokenRenewalJob
   ├─ Auto-renews expiring tokens
   ├─ Runs daily
   └─ Notifies on failure
```

**Why Hangfire:**
- Built-in job scheduling and retry logic
- Dashboard for monitoring sync jobs
- Persistent job queue (survives restarts)
- Automatic failure handling
- Easy to configure recurring jobs

#### 5. **Error Handling & Recovery**
**Gap**: What happens when sync fails or connection drops?

**Recommendations:**
- **Retry Logic**: Exponential backoff for failed requests
- **Partial Import**: Save progress, resume from last successful file
- **Error Notification**: Email admin on sync failures
- **Health Checks**: Periodic connectivity checks
- **Automatic Recovery**: Auto-retry failed syncs

```
Error Recovery:
├─ Connection Failed
│  └─ Retry: 3 attempts with exponential backoff
├─ Token Expired
│  └─ Auto-renew if within renewal window
├─ Partial Download
│  └─ Resume from last completed file
├─ Instance Offline
│  └─ Mark as inactive, retry later
└─ Import Failed
   └─ Log error, notify admin, allow manual retry
```

#### 6. **Conflict Resolution**
**Implementation Decision**: Create new version for updated federated models

**Approach:**
- **Model Updates**: Create new version rather than overwriting
- **Version Tracking**: Track model versions for change detection
- **Deletion Handling**: Keep local copy, mark as "orphaned"
- **Local Edits**: Federated models are read-only on importing instance

```
Update Scenarios:

1. Model Updated on Source (Most Common)
   ├─ Change detected by FederationChangeDetectionJob
   ├─ Download updated model metadata and files
   ├─ Create NEW VERSION of the local federated model
   ├─ Preserve previous version(s)
   ├─ Update LastFederationSync timestamp
   └─ Users see new version available

   Example:
   - Local has: "Miniature Tank v1" (from Instance B)
   - Source updates: "Miniature Tank v2" (on Instance B)
   - Result: Local now has both v1 and v2
   - Users can choose which version to download

2. Model Deleted on Source
   ├─ Keep Local Copy → Mark as "orphaned"
   ├─ Update IsFederated status: remains true
   ├─ Set RemoteStatus: "Deleted"
   ├─ Stop syncing updates for this model
   ├─ Display badge: "Original removed from source"
   └─ Users retain access to the model

3. Model Modified Locally (Federated Model Edited)
   ├─ Federated models are READ-ONLY
   ├─ Users cannot edit federated models directly
   ├─ Option: "Remix" or "Fork" creates new local model
   ├─ Forked model: IsFederated = false
   └─ Original federated model remains unchanged

4. New Version Created on Source
   ├─ Detected by change detection job
   ├─ Imported as new version
   ├─ All versions remain available
   ├─ Version history preserved
   └─ Attribution maintained
```

**Version Management:**
- Each model update from source creates a new ModelVersion
- Previous versions remain downloadable
- Version history shows federation source
- Users can see which version came from which sync

**Why Version Creation:**
- ✅ No data loss (all versions preserved)
- ✅ Users can choose their preferred version
- ✅ Clear audit trail of changes
- ✅ Matches existing PolyBucket versioning system
- ✅ Simpler than conflict resolution UI

#### 7. **Security Considerations**
**Gap**: Comprehensive security measures

**Recommendations:**
- **HTTPS Only**: Require HTTPS for all federation connections
- **Token Encryption**: Encrypt tokens at rest
- **Audit Logging**: Log all federation events
- **IP Whitelisting** (Optional): Limit connections to known IPs
- **Content Validation**: Validate downloaded files (checksums, antivirus)
- **CSRF Protection**: Use CSRF tokens for handshake endpoints
- **DDoS Protection**: Rate limiting and IP blocking

#### 8. **Monitoring & Observability**
**Implementation Decision**: Hangfire jobs for status monitoring and change detection

**Approach:**
- **Health Check Job**: Periodic connectivity and status checks
- **Change Detection Job**: Monitor for updated models on federated instances
- **Token Expiry Monitoring**: Track token expiration dates
- **Sync Status Dashboard**: Display federation health metrics
- **Automated Alerts**: Email notifications on failures

```
Hangfire Monitoring Jobs:

1. FederationHealthCheckJob (Runs: Every 15 minutes)
   ├─ Ping each federated instance (/api/federation/health)
   ├─ Update connection status (Online/Offline)
   ├─ Check token expiry (warn if < 7 days)
   ├─ Update LastActivityAt timestamp
   └─ Alert admin on failures

2. FederationChangeDetectionJob (Runs: Per instance schedule)
   ├─ Query remote instance for changes since LastSyncAt
   ├─ GET /api/federation/catalog/changes?since={timestamp}
   ├─ Identify new models
   ├─ Identify updated models (version changes)
   ├─ Queue models for import/update
   └─ Log detected changes

3. FederationTokenExpiryCheckJob (Runs: Daily)
   ├─ Check all federation tokens
   ├─ Identify tokens expiring soon (< 14 days)
   ├─ Send email notification to admin
   ├─ Auto-renew if enabled
   └─ Update token status

Monitoring Dashboard Metrics:
├─ Federation Health
│  ├─ Active Federations: 5
│  ├─ Pending Handshakes: 2
│  ├─ Offline Instances: 1
│  ├─ Tokens Expiring Soon: 3 (< 14 days)
│  └─ Failed Health Checks: 1
├─ Sync Status
│  ├─ Successful Syncs (24h): 4/5
│  ├─ Failed Syncs (24h): 1/5
│  ├─ Average Sync Duration: 15 min
│  ├─ Models Imported (24h): 150
│  ├─ Models Updated (24h): 25
│  └─ Models in Queue: 45
└─ Bandwidth Usage
   ├─ Today: 2.5GB / 10GB (25%)
   ├─ This Week: 15GB / 70GB (21%)
   ├─ Top Bandwidth Instance: Instance B (8GB)
   └─ Next Batch ETA: 12 minutes
```

**Change Detection API:**
```http
GET /api/federation/catalog/changes?since=2025-01-15T14:30:00Z
Authorization: Bearer {federation_token}

Response:
{
  "newModels": 15,
  "updatedModels": 8,
  "deletedModels": 2,
  "models": [
    {
      "id": "model-uuid",
      "action": "created|updated|deleted",
      "updatedAt": "2025-01-16T10:20:00Z",
      "version": 2
    }
  ]
}
```

#### 9. **User Experience**
**Gap**: How does this appear to end users?

**Recommendations:**
- **Federated Badges**: Clear visual indicators on model cards
- **Author Attribution**: Show original author + instance
- **Source Link**: Link back to original instance (if public)
- **Federation Filter**: "Show only federated" / "Hide federated" toggle
- **Performance**: Federated models should feel as fast as local (they are local!)

#### 10. **Compliance & Legal**
**Gap**: Legal and compliance considerations

**Recommendations:**
- **License Enforcement**: Only import models with compatible licenses
- **DMCA/Takedown**: Process for handling copyright claims
- **Privacy**: Federated user data handling (GDPR)
- **Terms Acceptance**: Instance admins accept federation ToS
- **Data Residency**: Consider data location requirements

### 🎯 Implementation Priority

**Phase 1: MVP Handshake (Weeks 1-2)**
1. ✅ Basic handshake flow (6 steps you outlined)
2. ✅ Simple token exchange (JWT)
3. ✅ Manual model selection UI
4. ✅ Local copy creation
5. ⚠️ Basic error handling
6. ⚠️ Simple state management

**Phase 2: Security & Reliability (Weeks 3-4)**
1. 🔒 Token encryption at rest
2. 🔒 Instance verification
3. 🔄 Retry logic and error recovery
4. 📝 Comprehensive audit logging
5. ⏱️ Handshake timeout and cleanup

**Phase 3: Automation & Monitoring (Weeks 5-6)**
1. ⚙️ Automatic sync configuration
2. ⚙️ Token auto-renewal
3. 📊 Health monitoring dashboard
4. 🚨 Error notifications
5. 📈 Bandwidth tracking

**Phase 4: Advanced Features (Weeks 7-8)**
1. 🎯 Conflict resolution UI
2. 🔍 Advanced filtering (category/license)
3. ⚡ Download queue optimization
4. 📉 Rate limiting per instance
5. 🌐 Multi-instance management UI

### 📋 Checklist: Have You Covered Everything?

- [x] Initial connection and acceptance/rejection
- [x] Cryptographic token exchange (bidirectional)
- [x] Token expiration and renewal
- [x] Model catalog exchange
- [x] Admin model selection UI
- [x] Local copy creation (models + users)
- [x] Manual handshake, automated sync distinction
- [ ] Instance verification mechanism
- [ ] Token encryption and secure storage
- [ ] Handshake state persistence and recovery
- [ ] Rate limiting and bandwidth management
- [ ] Error handling and retry logic
- [ ] Conflict resolution strategy
- [ ] Security measures (HTTPS, validation)
- [ ] Monitoring and health checks
- [ ] Audit logging
- [ ] Token revocation process
- [ ] Compliance and legal considerations

## 📋 Implementation Decisions Summary

Based on the discussion, here are the **finalized implementation decisions**:

### **1. Instance Verification**
✅ **Decision**: Primarily through cryptographic token exchange
- Trust established via JWT tokens (RS256)
- Instance metadata (URL, name, version) for display only
- Admin approval provides human oversight
- No complex verification infrastructure needed

### **2. Token Revocation & Federation Removal**
✅ **Decision**: Models remain, syncing stops, soft delete
- All imported models stay on both instances
- Federation records soft-deleted (IsDeleted = true)
- Sync jobs stopped immediately
- Attribution metadata preserved
- Re-federation possible later

### **3. Handshake Rejection & Model Syncing**
✅ **Decision**: Model syncing is optional, not required
- Instances can federate without model sharing
- Each instance independently decides what to import
- "Federated but not syncing" is a valid state
- One-way or bidirectional sync supported

### **4. Sync Filters**
✅ **Decision**: Comprehensive filtering available
- Filter by categories (select multiple)
- Filter by licenses (select multiple)
- Filter by file size (max MB)
- Admin-configurable per instance

### **5. Bandwidth Management**
✅ **Decision**: Hangfire jobs with admin-configurable batching
- **Batch Size**: 100 models per batch (configurable: 10-1000)
- **Batch Interval**: 20 minutes (configurable: 5-1440 min)
- **Job Scheduler**: Hangfire for reliability
- **Rate Limiting**: Admin-configurable per instance
- **Concurrent Downloads**: 5 files per batch

### **6. Status Monitoring**
✅ **Decision**: Hangfire jobs for health checks and change detection
- **Health Check Job**: Every 15 minutes
- **Change Detection Job**: Per instance schedule
- **Token Expiry Check**: Daily
- Monitor for new/updated models on remote instances
- Automated alerts on failures

### **7. Conflict Resolution**
✅ **Decision**: Create new version for federated model updates
- Model updates create new ModelVersion (not overwrite)
- Previous versions preserved
- Deleted models marked as "orphaned" (kept locally)
- Federated models are read-only
- Users can "Remix/Fork" to create editable copy

### **8. Re-Handshake Process**
✅ **Decision**: Button to ping for renewal; automatic/timed/manual options
- **Automatic Renewal**: Auto-renew 7 days before expiration
- **Timed Renewal**: Renewal at scheduled time
- **Manual Renewal**: Admin clicks "Refresh Handshake"
- Either instance can initiate renewal
- Full handshake repeats if expired
- Syncing resumes from last sync point

### 🚀 Ready to Implement?

Your handshake process is solid and covers the essential flow. The implementation decisions above provide clear guidance for production-ready features.

**Recommended MVP Scope:**
- ✅ 7-step handshake process (connection → import)
- ✅ JWT token exchange (RS256)
- ✅ Manual model selection UI
- ✅ Local copy creation (models + users)
- ✅ Hangfire job setup (sync, health, change detection)
- ✅ Basic filtering (category, license, size)
- ✅ Batch import configuration (size, interval)
- ✅ Token renewal (automatic/manual)
- ✅ Version creation for updates
- ✅ Basic error handling

**For Production Enhancement (Add Later):**
- Token encryption at rest
- Advanced retry logic
- Comprehensive monitoring dashboard
- Email notifications
- Compliance features
- WebSocket-based push updates

## Next Steps

1. **Review updated proposal** with handshake details
2. **Confirm MVP scope** (7-step handshake + basic features)
3. **Design database schema** for handshake tracking
4. **Create handshake API endpoints** following CQRS pattern
5. **Implement handshake UI** in admin panel
6. **Build token management** (generation, storage, renewal)
7. **Test handshake flow** between two test instances
8. **Iterate and enhance** based on testing feedback
