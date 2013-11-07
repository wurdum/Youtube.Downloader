//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)

//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2005-2010 Novell, Inc (http://novell.com/)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace Youtube.Downloader.Http
{
    public class HttpUtilities
    {
        #region static

        private static readonly object entitiesLock = new object();
        private static SortedDictionary<string, char> entities;

        private static IDictionary<string, char> Entities {
            get {
                lock (entitiesLock) {
                    if (entities == null)
                        InitEntities();

                    return entities;
                }
            }
        }

        private static void InitEntities() {
            entities = new SortedDictionary<string, char>(StringComparer.Ordinal) {
                {"nbsp", '\u00A0'},
                {"iexcl", '\u00A1'},
                {"cent", '\u00A2'},
                {"pound", '\u00A3'},
                {"curren", '\u00A4'},
                {"yen", '\u00A5'},
                {"brvbar", '\u00A6'},
                {"sect", '\u00A7'},
                {"uml", '\u00A8'},
                {"copy", '\u00A9'},
                {"ordf", '\u00AA'},
                {"laquo", '\u00AB'},
                {"not", '\u00AC'},
                {"shy", '\u00AD'},
                {"reg", '\u00AE'},
                {"macr", '\u00AF'},
                {"deg", '\u00B0'},
                {"plusmn", '\u00B1'},
                {"sup2", '\u00B2'},
                {"sup3", '\u00B3'},
                {"acute", '\u00B4'},
                {"micro", '\u00B5'},
                {"para", '\u00B6'},
                {"middot", '\u00B7'},
                {"cedil", '\u00B8'},
                {"sup1", '\u00B9'},
                {"ordm", '\u00BA'},
                {"raquo", '\u00BB'},
                {"frac14", '\u00BC'},
                {"frac12", '\u00BD'},
                {"frac34", '\u00BE'},
                {"iquest", '\u00BF'},
                {"Agrave", '\u00C0'},
                {"Aacute", '\u00C1'},
                {"Acirc", '\u00C2'},
                {"Atilde", '\u00C3'},
                {"Auml", '\u00C4'},
                {"Aring", '\u00C5'},
                {"AElig", '\u00C6'},
                {"Ccedil", '\u00C7'},
                {"Egrave", '\u00C8'},
                {"Eacute", '\u00C9'},
                {"Ecirc", '\u00CA'},
                {"Euml", '\u00CB'},
                {"Igrave", '\u00CC'},
                {"Iacute", '\u00CD'},
                {"Icirc", '\u00CE'},
                {"Iuml", '\u00CF'},
                {"ETH", '\u00D0'},
                {"Ntilde", '\u00D1'},
                {"Ograve", '\u00D2'},
                {"Oacute", '\u00D3'},
                {"Ocirc", '\u00D4'},
                {"Otilde", '\u00D5'},
                {"Ouml", '\u00D6'},
                {"times", '\u00D7'},
                {"Oslash", '\u00D8'},
                {"Ugrave", '\u00D9'},
                {"Uacute", '\u00DA'},
                {"Ucirc", '\u00DB'},
                {"Uuml", '\u00DC'},
                {"Yacute", '\u00DD'},
                {"THORN", '\u00DE'},
                {"szlig", '\u00DF'},
                {"agrave", '\u00E0'},
                {"aacute", '\u00E1'},
                {"acirc", '\u00E2'},
                {"atilde", '\u00E3'},
                {"auml", '\u00E4'},
                {"aring", '\u00E5'},
                {"aelig", '\u00E6'},
                {"ccedil", '\u00E7'},
                {"egrave", '\u00E8'},
                {"eacute", '\u00E9'},
                {"ecirc", '\u00EA'},
                {"euml", '\u00EB'},
                {"igrave", '\u00EC'},
                {"iacute", '\u00ED'},
                {"icirc", '\u00EE'},
                {"iuml", '\u00EF'},
                {"eth", '\u00F0'},
                {"ntilde", '\u00F1'},
                {"ograve", '\u00F2'},
                {"oacute", '\u00F3'},
                {"ocirc", '\u00F4'},
                {"otilde", '\u00F5'},
                {"ouml", '\u00F6'},
                {"divide", '\u00F7'},
                {"oslash", '\u00F8'},
                {"ugrave", '\u00F9'},
                {"uacute", '\u00FA'},
                {"ucirc", '\u00FB'},
                {"uuml", '\u00FC'},
                {"yacute", '\u00FD'},
                {"thorn", '\u00FE'},
                {"yuml", '\u00FF'},
                {"fnof", '\u0192'},
                {"Alpha", '\u0391'},
                {"Beta", '\u0392'},
                {"Gamma", '\u0393'},
                {"Delta", '\u0394'},
                {"Epsilon", '\u0395'},
                {"Zeta", '\u0396'},
                {"Eta", '\u0397'},
                {"Theta", '\u0398'},
                {"Iota", '\u0399'},
                {"Kappa", '\u039A'},
                {"Lambda", '\u039B'},
                {"Mu", '\u039C'},
                {"Nu", '\u039D'},
                {"Xi", '\u039E'},
                {"Omicron", '\u039F'},
                {"Pi", '\u03A0'},
                {"Rho", '\u03A1'},
                {"Sigma", '\u03A3'},
                {"Tau", '\u03A4'},
                {"Upsilon", '\u03A5'},
                {"Phi", '\u03A6'},
                {"Chi", '\u03A7'},
                {"Psi", '\u03A8'},
                {"Omega", '\u03A9'},
                {"alpha", '\u03B1'},
                {"beta", '\u03B2'},
                {"gamma", '\u03B3'},
                {"delta", '\u03B4'},
                {"epsilon", '\u03B5'},
                {"zeta", '\u03B6'},
                {"eta", '\u03B7'},
                {"theta", '\u03B8'},
                {"iota", '\u03B9'},
                {"kappa", '\u03BA'},
                {"lambda", '\u03BB'},
                {"mu", '\u03BC'},
                {"nu", '\u03BD'},
                {"xi", '\u03BE'},
                {"omicron", '\u03BF'},
                {"pi", '\u03C0'},
                {"rho", '\u03C1'},
                {"sigmaf", '\u03C2'},
                {"sigma", '\u03C3'},
                {"tau", '\u03C4'},
                {"upsilon", '\u03C5'},
                {"phi", '\u03C6'},
                {"chi", '\u03C7'},
                {"psi", '\u03C8'},
                {"omega", '\u03C9'},
                {"thetasym", '\u03D1'},
                {"upsih", '\u03D2'},
                {"piv", '\u03D6'},
                {"bull", '\u2022'},
                {"hellip", '\u2026'},
                {"prime", '\u2032'},
                {"Prime", '\u2033'},
                {"oline", '\u203E'},
                {"frasl", '\u2044'},
                {"weierp", '\u2118'},
                {"image", '\u2111'},
                {"real", '\u211C'},
                {"trade", '\u2122'},
                {"alefsym", '\u2135'},
                {"larr", '\u2190'},
                {"uarr", '\u2191'},
                {"rarr", '\u2192'},
                {"darr", '\u2193'},
                {"harr", '\u2194'},
                {"crarr", '\u21B5'},
                {"lArr", '\u21D0'},
                {"uArr", '\u21D1'},
                {"rArr", '\u21D2'},
                {"dArr", '\u21D3'},
                {"hArr", '\u21D4'},
                {"forall", '\u2200'},
                {"part", '\u2202'},
                {"exist", '\u2203'},
                {"empty", '\u2205'},
                {"nabla", '\u2207'},
                {"isin", '\u2208'},
                {"notin", '\u2209'},
                {"ni", '\u220B'},
                {"prod", '\u220F'},
                {"sum", '\u2211'},
                {"minus", '\u2212'},
                {"lowast", '\u2217'},
                {"radic", '\u221A'},
                {"prop", '\u221D'},
                {"infin", '\u221E'},
                {"ang", '\u2220'},
                {"and", '\u2227'},
                {"or", '\u2228'},
                {"cap", '\u2229'},
                {"cup", '\u222A'},
                {"int", '\u222B'},
                {"there4", '\u2234'},
                {"sim", '\u223C'},
                {"cong", '\u2245'},
                {"asymp", '\u2248'},
                {"ne", '\u2260'},
                {"equiv", '\u2261'},
                {"le", '\u2264'},
                {"ge", '\u2265'},
                {"sub", '\u2282'},
                {"sup", '\u2283'},
                {"nsub", '\u2284'},
                {"sube", '\u2286'},
                {"supe", '\u2287'},
                {"oplus", '\u2295'},
                {"otimes", '\u2297'},
                {"perp", '\u22A5'},
                {"sdot", '\u22C5'},
                {"lceil", '\u2308'},
                {"rceil", '\u2309'},
                {"lfloor", '\u230A'},
                {"rfloor", '\u230B'},
                {"lang", '\u2329'},
                {"rang", '\u232A'},
                {"loz", '\u25CA'},
                {"spades", '\u2660'},
                {"clubs", '\u2663'},
                {"hearts", '\u2665'},
                {"diams", '\u2666'},
                {"quot", '\u0022'},
                {"amp", '\u0026'},
                {"lt", '\u003C'},
                {"gt", '\u003E'},
                {"OElig", '\u0152'},
                {"oelig", '\u0153'},
                {"Scaron", '\u0160'},
                {"scaron", '\u0161'},
                {"Yuml", '\u0178'},
                {"circ", '\u02C6'},
                {"tilde", '\u02DC'},
                {"ensp", '\u2002'},
                {"emsp", '\u2003'},
                {"thinsp", '\u2009'},
                {"zwnj", '\u200C'},
                {"zwj", '\u200D'},
                {"lrm", '\u200E'},
                {"rlm", '\u200F'},
                {"ndash", '\u2013'},
                {"mdash", '\u2014'},
                {"lsquo", '\u2018'},
                {"rsquo", '\u2019'},
                {"sbquo", '\u201A'},
                {"ldquo", '\u201C'},
                {"rdquo", '\u201D'},
                {"bdquo", '\u201E'},
                {"dagger", '\u2020'},
                {"Dagger", '\u2021'},
                {"permil", '\u2030'},
                {"lsaquo", '\u2039'},
                {"rsaquo", '\u203A'},
                {"euro", '\u20AC'}
            };
        }

        #endregion

        public string HtmlDecode(string s) {
            if (s == null)
                return null;

            if (s.Length == 0)
                return String.Empty;

            if (s.IndexOf('&') == -1)
                return s;

            var entity = new StringBuilder();
            var output = new StringBuilder();
            var len = s.Length;
            // 0 -> nothing,
            // 1 -> right after '&'
            // 2 -> between '&' and ';' but no '#'
            // 3 -> '#' found after '&' and getting numbers
            int state = 0;
            int number = 0;
            bool is_hex_value = false;
            bool have_trailing_digits = false;

            for (int i = 0; i < len; i++) {
                char c = s[i];
                if (state == 0) {
                    if (c == '&') {
                        entity.Append(c);
                        state = 1;
                    } else {
                        output.Append(c);
                    }
                    continue;
                }

                if (c == '&') {
                    state = 1;
                    if (have_trailing_digits) {
                        entity.Append(number.ToString(CultureInfo.InvariantCulture));
                        have_trailing_digits = false;
                    }

                    output.Append(entity);
                    entity.Length = 0;
                    entity.Append('&');
                    continue;
                }

                if (state == 1) {
                    if (c == ';') {
                        state = 0;
                        output.Append(entity);
                        output.Append(c);
                        entity.Length = 0;
                    } else {
                        number = 0;
                        is_hex_value = false;
                        if (c != '#') {
                            state = 2;
                        } else {
                            state = 3;
                        }
                        entity.Append(c);
                    }
                } else if (state == 2) {
                    entity.Append(c);
                    if (c == ';') {
                        string key = entity.ToString();
                        if (key.Length > 1 && Entities.ContainsKey(key.Substring(1, key.Length - 2)))
                            key = Entities[key.Substring(1, key.Length - 2)].ToString();

                        output.Append(key);
                        state = 0;
                        entity.Length = 0;
                    }
                } else if (state == 3) {
                    if (c == ';') {
                        if (number > 65535) {
                            output.Append("&#");
                            output.Append(number.ToString(CultureInfo.InvariantCulture));
                            output.Append(";");
                        } else {
                            output.Append((char) number);
                        }
                        state = 0;
                        entity.Length = 0;
                        have_trailing_digits = false;
                    } else if (is_hex_value && Uri.IsHexDigit(c)) {
                        number = number*16 + Uri.FromHex(c);
                        have_trailing_digits = true;
                    } else if (Char.IsDigit(c)) {
                        number = number*10 + (c - '0');
                        have_trailing_digits = true;
                    } else if (number == 0 && (c == 'x' || c == 'X')) {
                        is_hex_value = true;
                    } else {
                        state = 2;
                        if (have_trailing_digits) {
                            entity.Append(number.ToString(CultureInfo.InvariantCulture));
                            have_trailing_digits = false;
                        }
                        entity.Append(c);
                    }
                }
            }

            if (entity.Length > 0) {
                output.Append(entity);
            } else if (have_trailing_digits) {
                output.Append(number.ToString(CultureInfo.InvariantCulture));
            }
            return output.ToString();
        }

        public string UrlDecode(string s, Encoding e) {
            if (null == s)
                return null;

            if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
                return s;

            if (e == null)
                e = Encoding.UTF8;

            long len = s.Length;
            var bytes = new List<byte>();
            int xchar;
            char ch;

            for (int i = 0; i < len; i++) {
                ch = s[i];
                if (ch == '%' && i + 2 < len && s[i + 1] != '%') {
                    if (s[i + 1] == 'u' && i + 5 < len) {
                        // unicode hex sequence
                        xchar = GetChar(s, i + 2, 4);
                        if (xchar != -1) {
                            WriteCharBytes(bytes, (char)xchar, e);
                            i += 5;
                        } else
                            WriteCharBytes(bytes, '%', e);
                    } else if ((xchar = GetChar(s, i + 1, 2)) != -1) {
                        WriteCharBytes(bytes, (char)xchar, e);
                        i += 2;
                    } else {
                        WriteCharBytes(bytes, '%', e);
                    }
                    continue;
                }

                if (ch == '+')
                    WriteCharBytes(bytes, ' ', e);
                else
                    WriteCharBytes(bytes, ch, e);
            }

            byte[] buf = bytes.ToArray();
            bytes = null;
            return e.GetString(buf);

        }

        public NameValueCollection ParseQueryString(string query, Encoding encoding) {
            if (query == null)
                throw new ArgumentNullException("query");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
                return new HttpQsCollection();
            if (query[0] == '?')
                query = query.Substring(1);

            NameValueCollection result = new HttpQsCollection();
            ParseQueryString(query, encoding, result);
            return result;
        }

        private int GetChar(string str, int offset, int length) {
            int val = 0;
            int end = length + offset;
            for (int i = offset; i < end; i++) {
                char c = str[i];
                if (c > 127)
                    return -1;

                int current = GetInt((byte)c);
                if (current == -1)
                    return -1;
                val = (val << 4) + current;
            }

            return val;
        }

        private int GetInt(byte b) {
            char c = (char)b;
            if (c >= '0' && c <= '9')
                return c - '0';

            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;

            return -1;
        }

        private void WriteCharBytes(IList buf, char ch, Encoding e) {
            if (ch > 255) {
                foreach (byte b in e.GetBytes(new char[] { ch }))
                    buf.Add(b);
            } else
                buf.Add((byte)ch);
        }

        private void ParseQueryString(string query, Encoding encoding, NameValueCollection result) {
            if (query.Length == 0)
                return;

            string decoded = HtmlDecode(query);
            int decodedLength = decoded.Length;
            int namePos = 0;
            bool first = true;
            while (namePos <= decodedLength) {
                int valuePos = -1, valueEnd = -1;
                for (int q = namePos; q < decodedLength; q++) {
                    if (valuePos == -1 && decoded[q] == '=') {
                        valuePos = q + 1;
                    } else if (decoded[q] == '&') {
                        valueEnd = q;
                        break;
                    }
                }

                if (first) {
                    first = false;
                    if (decoded[namePos] == '?')
                        namePos++;
                }

                string name, value;
                if (valuePos == -1) {
                    name = null;
                    valuePos = namePos;
                } else {
                    name = UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1), encoding);
                }
                if (valueEnd < 0) {
                    namePos = -1;
                    valueEnd = decoded.Length;
                } else {
                    namePos = valueEnd + 1;
                }
                value = UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos), encoding);

                result.Add(name, value);
                if (namePos == -1)
                    break;
            }
        }

        sealed class HttpQsCollection : NameValueCollection
        {
            public override string ToString() {
                int count = Count;
                if (count == 0)
                    return "";
                StringBuilder sb = new StringBuilder();
                string[] keys = AllKeys;
                for (int i = 0; i < count; i++) {
                    sb.AppendFormat("{0}={1}&", keys[i], this[keys[i]]);
                }
                if (sb.Length > 0)
                    sb.Length--;
                return sb.ToString();
            }
        }
    }
}