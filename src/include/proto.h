#pragma once
#include <cstdint>
#include <cstddef>

namespace Proto {

struct Field {
    uint32_t       num;
    int            wt;
    uint64_t       u64;
    const uint8_t* ptr;
    size_t         len;
};

namespace detail {

inline bool ReadVarint(const uint8_t* buf, size_t total,
                       size_t& pos, uint64_t& out) {
    out = 0;
    int shift = 0;
    while (pos < total && shift < 64) {
        uint8_t b = buf[pos++];
        out |= static_cast<uint64_t>(b & 0x7F) << shift;
        if (!(b & 0x80)) return true;
        shift += 7;
    }
    return false;
}

inline bool Skip(const uint8_t* buf, size_t total, size_t& pos, int wt) {
    uint64_t tmp = 0;
    switch (wt) {
        case 0: return ReadVarint(buf, total, pos, tmp);
        case 1: if (pos + 8 > total) return false; pos += 8; return true;
        case 2: if (!ReadVarint(buf, total, pos, tmp)) return false;
                if (pos + static_cast<size_t>(tmp) > total) return false;
                pos += static_cast<size_t>(tmp); return true;
        case 5: if (pos + 4 > total) return false; pos += 4; return true;
        default: return false;
    }
}

}

template<typename Fn>
inline void Walk(const uint8_t* buf, size_t len, Fn&& cb) {
    size_t pos = 0;
    while (pos < len) {
        uint64_t tag = 0;
        if (!detail::ReadVarint(buf, len, pos, tag)) break;
        const uint32_t fn = static_cast<uint32_t>(tag >> 3);
        const int      wt = static_cast<int>(tag & 7);
        if (fn == 0) break;
        if (wt == 0) {
            Field f{fn, wt, 0, nullptr, 0};
            if (!detail::ReadVarint(buf, len, pos, f.u64)) break;
            if (!cb(f)) return;
        } else if (wt == 2) {
            uint64_t l = 0;
            if (!detail::ReadVarint(buf, len, pos, l)) break;
            if (pos + static_cast<size_t>(l) > len) break;
            Field f{fn, wt, 0, buf + pos, static_cast<size_t>(l)};
            pos += static_cast<size_t>(l);
            if (!cb(f)) return;
        } else if (wt == 5) {
            if (pos + 4 > len) break;
            uint32_t v = static_cast<uint32_t>(buf[pos])
                       | static_cast<uint32_t>(buf[pos + 1]) << 8
                       | static_cast<uint32_t>(buf[pos + 2]) << 16
                       | static_cast<uint32_t>(buf[pos + 3]) << 24;
            pos += 4;
            Field f{fn, wt, v, nullptr, 0};
            if (!cb(f)) return;
        } else if (wt == 1) {
            if (pos + 8 > len) break;
            uint64_t v = 0;
            for (int i = 0; i < 8; ++i)
                v |= static_cast<uint64_t>(buf[pos + i]) << (i * 8);
            pos += 8;
            Field f{fn, wt, v, nullptr, 0};
            if (!cb(f)) return;
        } else {
            if (!detail::Skip(buf, len, pos, wt)) break;
        }
    }
}

}
