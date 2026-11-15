#!/usr/bin/python3

from sanitise import *
from pedigree import *

def generate_orchestration_messages(namespace, prefix, orchestration):
    sane_prefix = sanitise_for_include_guard(prefix)
    filename = '{}Messages.cs'.format(prefix)
    with open(filename, 'w') as file:
        file.write('namespace Fix;\n\n')
        file.write('public partial class Dictionary\n{\n')
        file.write('    public partial class {}\n'.format(namespace))
        file.write('    {\n')
        file.write('        public partial class Message\n')
        file.write('        {\n')
        for message in orchestration.messages.values():
            file.write('\n')
            file.write('            public class {} : Dictionary.Message\n'.format(message.name))
            file.write('            {\n')
            file.write('                internal {}()\n'.format(message.name)) 
            file.write('                : base(\n')
            file.write('                    "{}",\n'.format(message.msg_type))
            file.write('                    "{}",\n'.format(message.name))
            file.write('                    "{}",\n'.format(sanitise(message.synopsis)))
            file.write('                    {},\n'.format(format_pedigree(message.pedigree)))
         
            file.write('                    new MessageFieldCollection(\n')

            initialisers = []
            for field in orchestration.message_fields(message):
                required = 'false'
                if field.presence and field.presence.lower() == 'required':
                    required = 'true'
                # Reference the shared field definition rather than constructing a new copy for every
                # field of every message.
                initialiser = '                        new MessageField({}.Fields.{}, {}, {}),\n'.format(namespace, field.field.name, required, field.depth)
                initialisers.append(initialiser)
            last = initialisers[len(initialisers) - 1]
            initialisers = initialisers[:-1]
            initialisers.append(last[:-2] + '\n')    
            
            for initialiser in initialisers:
                file.write(initialiser)

            file.write('                    )\n')
            file.write('                  )\n')
            file.write('                {\n')
            file.write('                }\n')
            file.write('            }\n')
        file.write('        }\n')


        file.write('\n')
        file.write('        public class MessageCollection : VersionMessageCollection\n')
        file.write('        {\n')
        file.write('            public MessageCollection()\n')
        file.write('            {\n')

        file.write('                Messages = new Dictionary.Message[] {{{}}};\n'.format(', '.join([message.name for message in orchestration.messages.values()])))

        file.write('            }\n\n')

        for message in orchestration.messages.values():
            file.write('            public readonly Dictionary.Message {} = new {}.Message.{}();\n'.format(message.name, namespace, message.name))

        file.write('        }\n')

        file.write('\n')
        file.write('        public static MessageCollection Messages { get; } = new MessageCollection();\n')


        file.write('    }\n')
        file.write('}\n')
