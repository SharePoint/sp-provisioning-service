import {
    Injectable, Component, NgModule, Compiler, NgModuleFactory, ViewEncapsulation, InjectionToken, Inject
} from '@angular/core';

const DYNAMIC_COMPONENT_DATA = new InjectionToken<any>('DynamicComponentData');

interface DynamicComponentOptions {
    /**
     * The html template for the component.
     */
    html: string;

    /**
     * The styles for the component.
     */
    css?: string;

    /**
     * View encapsulation method to use for the styles.
     */
    encapsulation?: ViewEncapsulation;
}

@Injectable({
    providedIn: 'root'
})
export class ComponentGeneratorService {
    constructor(
        private compiler: Compiler
    ) { }

    /**
     * Cretes a component for the given html and css
     * @param options
     */
    createComponent(options: DynamicComponentOptions): any {
        const { html } = options;
        let { css, encapsulation } = options;

        if (!html) {
            throw new Error('HTML must be provided for template');
        }

        // If css is not provided, make empty string
        css = css || '';
        encapsulation = encapsulation || ViewEncapsulation.Emulated;

        @Component({
            template: html,
            styles: [css],
            encapsulation
        })
        class DynamicComponent {
            constructor(
                @Inject(DYNAMIC_COMPONENT_DATA) public context: any
            ) { }
        }
        return DynamicComponent;
    }

    /**
     * Creates a runtime module factory for the given component
     * @param componentType
     * @param moduleDefinition
     * @param context
     */
    createModuleForComponent(componentType: any, moduleDefinition: NgModule, context?: any): NgModuleFactory<any> {
        if (typeof componentType !== 'function') {
            throw new Error('Invalid component provided');
        }

        if (moduleDefinition) {
            // Need to make sure this is a new object, or we could end up with error
            moduleDefinition = Object.assign({}, moduleDefinition);
        }
        moduleDefinition = moduleDefinition || {};

        moduleDefinition.imports = moduleDefinition.imports || [];
        moduleDefinition.declarations = moduleDefinition.declarations || [];
        moduleDefinition.entryComponents = moduleDefinition.entryComponents || [];

        moduleDefinition.declarations.push(componentType);
        moduleDefinition.entryComponents.push(componentType);

        moduleDefinition.providers = moduleDefinition.providers || [];
        moduleDefinition.providers.push(
            { provide: DYNAMIC_COMPONENT_DATA, useValue: context }
        );

        @NgModule(moduleDefinition)
        class RuntimeModule { }

        return this.compiler.compileModuleSync(RuntimeModule);
    }
}
