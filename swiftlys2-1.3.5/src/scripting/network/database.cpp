/************************************************************************************************
 *  SwiftlyS2 is a scripting framework for Source2-based games.
 *  Copyright (C) 2023-2026 Swiftly Solution SRL via Sava Andrei-Sebastian and it's contributors
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 ************************************************************************************************/

#include <api/interfaces/manager.h>
#include <scripting/scripting.h>

static char* Bridge_Database_CopyString(const std::string& value, int* size)
{
    static auto memory = g_ifaceService.FetchInterface<IMemoryAllocator>(MEMORYALLOCATOR_INTERFACE_VERSION);

    int outSize = static_cast<int>(value.size());
    *size = outSize;

    char* out = (char*)memory->Alloc(outSize + 1);
    memory->Copy(out, (void*)value.c_str(), outSize);
    out[outSize] = '\0';
    return out;
}

char* Bridge_Database_GetDefaultDriver(int* size)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetDefaultDriver();

    return Bridge_Database_CopyString(o, size);
}

char* Bridge_Database_GetDefaultConnectionName(int* size)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetDefaultConnectionName();

    return Bridge_Database_CopyString(o, size);
}

char* Bridge_Database_GetConnectionDriver(int* size, const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetConnection(connectionName).driver;
    return Bridge_Database_CopyString(o, size);
}

char* Bridge_Database_GetConnectionHost(int* size, const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetConnection(connectionName).host;
    return Bridge_Database_CopyString(o, size);
}

char* Bridge_Database_GetConnectionDatabase(int* size, const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetConnection(connectionName).database;
    return Bridge_Database_CopyString(o, size);
}

char* Bridge_Database_GetConnectionUser(int* size, const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetConnection(connectionName).user;
    return Bridge_Database_CopyString(o, size);
}

char* Bridge_Database_GetConnectionPass(int* size, const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetConnection(connectionName).pass;
    return Bridge_Database_CopyString(o, size);
}

uint32_t Bridge_Database_GetConnectionTimeout(const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    auto conn = db->GetConnection(connectionName);
    return conn.timeout;
}

uint16_t Bridge_Database_GetConnectionPort(const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    auto conn = db->GetConnection(connectionName);
    return conn.port;
}

char* Bridge_Database_GetConnectionRawUri(int* size, const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    std::string o = db->GetConnection(connectionName).rawUri;

    return Bridge_Database_CopyString(o, size);
}

bool Bridge_Database_ConnectionExists(const char* connectionName)
{
    static auto db = g_ifaceService.FetchInterface<IDatabaseManager>(DATABASEMANAGER_INTERFACE_VERSION);
    return db->ConnectionExists(connectionName);
}

DEFINE_NATIVE("Database.GetDefaultDriver", Bridge_Database_GetDefaultDriver);
DEFINE_NATIVE("Database.GetDefaultConnectionName", Bridge_Database_GetDefaultConnectionName);
DEFINE_NATIVE("Database.GetConnectionDriver", Bridge_Database_GetConnectionDriver);
DEFINE_NATIVE("Database.GetConnectionHost", Bridge_Database_GetConnectionHost);
DEFINE_NATIVE("Database.GetConnectionDatabase", Bridge_Database_GetConnectionDatabase);
DEFINE_NATIVE("Database.GetConnectionUser", Bridge_Database_GetConnectionUser);
DEFINE_NATIVE("Database.GetConnectionPass", Bridge_Database_GetConnectionPass);
DEFINE_NATIVE("Database.GetConnectionTimeout", Bridge_Database_GetConnectionTimeout);
DEFINE_NATIVE("Database.GetConnectionPort", Bridge_Database_GetConnectionPort);
DEFINE_NATIVE("Database.GetConnectionRawUri", Bridge_Database_GetConnectionRawUri);
DEFINE_NATIVE("Database.ConnectionExists", Bridge_Database_ConnectionExists);