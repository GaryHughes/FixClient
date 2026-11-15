#!/usr/bin/python3

from sanitise import *
from pedigree import *

def generate_orchestration_fields(namespace, prefix, orchestration):
    sorted_fields = sorted(orchestration.fields_by_tag.values(), key=lambda x: int(x.id))
    sane_prefix = sanitise_for_include_guard(prefix)
    filename = '{}Fields.cs'.format(prefix)
    with open(filename, 'w') as file:
        file.write('namespace Fix;\n\n')
        file.write('public partial class Dictionary\n')
        file.write('{\n')
        file.write('    public partial class {}\n'.format(namespace))
        file.write('    {\n')

        for field in sorted_fields:
     
            values = []
            try:
                code_set = orchestration.code_sets[field.type]
                file.write('\n')
                for code in code_set.codes:
                    name = code.name
                    if name == field.name:
                        print('{}.{} would result in a class member having the same name as the class which is invalid C#, renaming to {}.{}_'.format(field.name, code.name, field.name, code.name))
                        name = name + '_'
                    if name in ('char', 'int', 'float', 'double'):
                        before = name
                        name = name[0].upper() + name[1:]
                        print('renaming field value from {} to {} which is a builtin C# typename'.format(before, name))
                    values.append((name, code.value, code.synopsis))
            except KeyError:
                # TODO - maybe check that its an expected built in type
                pass
        
            file.write('\n')
            file.write('        public class {} : Fix.Dictionary.VersionField\n'.format(field.name))
            file.write('        {\n')
            file.write('            public {}()\n'.format(field.name))
            file.write('            : base({}, "{}", "{}", "{}", {}'.format(field.id, field.name, field.type, sanitise(field.synopsis), format_pedigree(field.pedigree)))
            if len(values):
                file.write(', ' + ', '.join([name for (name, _, _) in values]))
            file.write(')\n')
            file.write('            {\n')
            file.write('            }\n')

            for (name, value, synopsis) in values:
                file.write('                public static readonly FieldValue {} = new FieldValue({}, "{}", "{}", "{}", {});\n'.format(name, field.id, name, value, sanitise(synopsis), format_pedigree(field.pedigree)))
            
            file.write('        }\n')

        file.write('\n')
        file.write('        public class FieldCollection : VersionFieldCollection\n')
        file.write('        {\n')

        file.write('            public FieldCollection()\n')
        file.write('            {\n')
        file.write('                Fields = new Dictionary.VersionField[] {\n')
        index = 0
        for field in sorted_fields:
            while field.id > index:
                file.write('                    VersionField.Dummy,\n')
                index += 1
            file.write('                    {},\n'.format(field.name))
            index += 1
        file.write('                };\n')
        file.write('            }\n\n')

        for field in sorted_fields:
            file.write('            public readonly VersionField {} = new {}.{}();\n'.format(field.name, namespace, field.name))

        file.write('        }\n')

        file.write('\n')
        # The generated messages reference these shared field definitions, so make sure they exist
        # whatever order the static initialisers of the partial class run in.
        file.write('        public static FieldCollection Fields => FieldCollectionInstance.Value;\n')
        file.write('\n')
        file.write('        static class FieldCollectionInstance\n')
        file.write('        {\n')
        file.write('            internal static readonly FieldCollection Value = new FieldCollection();\n')
        file.write('        }\n')

        file.write('    }\n')
        file.write('}\n')
