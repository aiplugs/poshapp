const parser = new DOMParser();

function parseXML(xml) {
    return parser.parseFromString(xml, 'application/xml');
}

const integers = ['By', 'I16', 'I32', 'I64', 'U16', 'U32', 'U64'];
const floats = ['Db','Sg','D'];

const utf16decoder = new TextDecoder('utf-16');
function decodeUTF16(text) {
    return text.replace(/_x([0-9A-F]{4})__x([0-9A-F]{4})_/g, function (_, a, b) {
        var binary = new Uint8Array([
            parseInt(a.slice(2), 16),
            parseInt(a.slice(0, 2), 16),
            parseInt(b.slice(2), 16),
            parseInt(b.slice(0, 2), 16)
        ]);
        return utf16decoder.decode(binary);
    });
}

function extract(element) {
    const tagName = element.tagName;

    if (tagName === 'Objs') {
        return extract(element.firstElementChild);
    }
    
    else if (tagName === 'Obj') {
        let types = Array.from(element.querySelectorAll(':scope > TN > T'));
        const tnref= element.querySelector(':scope > TNRef');
        if (types.length === 0 && tnref) {
            const refId = tnref.getAttribute('RefId');
            types = element.parentElement.querySelectorAll(`TN[RefId="${refId}"] > T`);
        }

        if (types.length === 0 && element.children.length > 0) {
            return extract(element.firstElementChild);
        }

        for (let t of types) {
            const typeName = t.textContent;
            
            if (typeName === 'System.DateTimeOffset') {
                const dt = element.querySelector('DT[N="LocalDateTime"]');
                return new Date(dt.textContent).toLocaleString();
            }
            if (typeName === 'System.ValueType') {
                const el = element.querySelector('ToString');
                return el !== null ? decodeUTF16(el.textContent) : null;
            }
            if (typeName === 'System.Array') {
                const lst = element.querySelector('LST');
                return Array.from(lst.children).map(el => extract(el));
            }
            if (typeName === 'System.Collections.Hashtable'||
                typeName === 'System.Collections.Specialized.OrderedDictionary'||
                typeName === 'Deserialized.System.Collections.Specialized.OrderedDictionary'||
                typeName.startsWith('System.Collections.Generic.Dictionary')) {
                const dct = element.querySelector('DCT');
                return Array.from(dct.children)
                            .map(el => {
                                const key = extract(el.querySelector('[N="Key"]'));
                                const value = extract(el.querySelector('[N="Value"]'));
                                return { key, value };
                            })
                            .reduce((o, kv) => { o[kv.key] = kv.value; return o; }, {});
            }
            if (typeName === 'System.Management.Automation.PSCustomObject') {
                const ms = element.querySelector('MS');
                return Array.from(ms.children)
                    .reduce((o, p) => {
                        const key = p.getAttribute('N');
                        const value = extract(p);
                        o[key] = value;
                        return o;
                    }, {});
            }
            if (typeName === 'System.Enum' || typeName === 'Deserialized.System.Enum') {
                const tostring = element.querySelector('ToString');
                return tostring !== null ? decodeUTF16(tostring.textContent) : null;
            }
            if (typeName === 'System.Object' ||
                typeName === 'Deserialized.System.Object') {
                const props = element.querySelector('Props');
                if (props) {
                    return Array.from(props.children)
                            .map(el => ({ key:el.getAttribute('N'), value: extract(el) }))
                            .reduce((o, kv) => { o[kv.key] = kv.value; return o; }, {});
                }
                
                const tostring = element.querySelector('ToString');
                return tostring !== null ? decodeUTF16(tostring.textContent) : null;
            }
        }
    }
    
    else if (tagName === 'S') {
        return decodeUTF16(element.textContent);
    }
    
    else if (tagName === 'C') {
        return String.fromCharCode(parseInt(element.textContent) || 0);
    }

    else if (tagName === 'B') {
        return element.textContent;
    }

    else if (tagName === 'DT') {
        if (element.textContent.endsWith("Z")) {
            return new Date(element.textContent).toLocaleString(undefined, { timeZone: 'UTC' });
        }
        return new Date(element.textContent).toLocaleString();
    }
    
    else if (integers.some(_ => _ === tagName)) {
        return parseInt(element.textContent);
    }

    else if (floats.some(_ => _ === tagName)) {
        return parseFloat(element.textContent);
    }

    return null;
}

export function parsePSDataCollection(data) {
    if (!Array.isArray(data))
        throw new Error("Argument data need array.");

    return data.map(d => {
        const clixml = d.CliXml;
        const objs = parseXML(clixml).querySelector('Objs');
        const value = extract(objs);
        
        return { value, clixml };
    });
}

const typeMap = {
    'System.String': 'S',
    'System.Byte': 'By',
    'System.Int16': 'I16',
    'System.Int32': 'I32',
    'System.Int64': 'I64',
    'System.UInt16': 'U16',
    'System.UInt32': 'U32',
    'System.UInt64': 'U64',
    'System.Char': 'C',
    'System.Boolean': 'B',
    'System.Single': 'Sg',
    'System.Double': 'Db',
    'System.Decimal': 'D',
    'System.DateTime': 'DT',
    'System.DateTimeOffset': 'DT'
};

export function createCliXml(type, value) {
    const tag = typeMap[type];
    const content = value === undefined || value === null? '<Nil />' : `<${tag}>${value}</${tag}>`;
    return `<Objs Version="1.1.0.1" xmlns="http://schemas.microsoft.com/powershell/2004/04">${content}</Objs>`;
}