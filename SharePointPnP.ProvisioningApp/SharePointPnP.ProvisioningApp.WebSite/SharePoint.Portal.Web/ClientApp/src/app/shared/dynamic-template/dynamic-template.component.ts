import { Component, Input, NgModule, NgModuleFactory, OnChanges, ViewEncapsulation, Output, EventEmitter } from '@angular/core';

import { environment } from 'src/environments/environment';
import { ComponentGeneratorService } from './component-generator.service';

@Component({
    selector: 'app-dynamic-template',
    templateUrl: './dynamic-template.component.html',
})
export class DynamicTemplateComponent implements OnChanges {
    @Input() html: string;
    @Input() css: string;
    @Input() context: string;
    @Input() moduleDefinition: NgModule;
    @Input() encapsulation: ViewEncapsulation;

    @Output() error = new EventEmitter<any>();

    component: any;
    moduleFactory: NgModuleFactory<any>;

    constructor(
        private componentGenerator: ComponentGeneratorService
    ) { }

    ngOnChanges() {
        this.generateNewComponent();
    }

    private generateNewComponent() {
        try {
            this.component = this.componentGenerator.createComponent({
                html: this.html,
                css: this.css,
                encapsulation: this.encapsulation
            });
            this.moduleFactory = this.componentGenerator.createModuleForComponent(
                this.component,
                this.moduleDefinition,
                this.context
            );
        } catch (e) {
            if (!environment.production) {
                console.error(e);
            }
            this.error.next(e);
        }
    }
}
