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

#include "allocator.h"
#include <cstring>
#include <algorithm>

#include "tier0/memdbgon.h"

void* MemoryAllocator::Alloc(uint64_t size)
{
    return malloc(size);
}

void MemoryAllocator::Free(void* ptr)
{
    free(ptr);
}

void* MemoryAllocator::Resize(void* ptr, uint64_t newSize)
{
    return realloc(ptr, newSize);
}

uint64_t MemoryAllocator::GetSize(void* ptr)
{
    return _msize(ptr);
}

uint64_t MemoryAllocator::GetTotalAllocated()
{
    return totalAllocated;
}

bool MemoryAllocator::IsPointerValid(void* ptr)
{
    return true;
}

void MemoryAllocator::Copy(void* dest, void* src, uint64_t size)
{
    memcpy(dest, src, size);
}

void MemoryAllocator::Move(void* dest, void* src, uint64_t size)
{
    memmove(dest, src, size);
}

MemoryAllocator::~MemoryAllocator()
{
}