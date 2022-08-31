import { FC, useState } from 'react';
import classes from './ResourceRegistry.module.css';
import { TextField, Button } from '@altinn/altinn-design-system';

export interface ResourceRegistryProps {
  id: string;
  title: string;
  organisationNumber: string;
  orgCode: string;
  telephone: string;
  email: string;
  isPartOf: string;
  hasPart: string;
  homepage: string;
  type: string[];
  sector: string[];
}

const ResourceRegistry: FC<ResourceRegistryProps> = () => {
  const idOptions = ['Id 1', 'Id 2', 'Id 3'];
  const titles = ['Title 1, Title 2', 'Title 3'];

  const resourceFormResult = {
    identifier: '',
    description: {
      nb: '',
    },
    title: {
      nb: '',
    },
    hasCompetentAuthority: {
      organization: '',
      orgCode: '',
    },
    contactPoint: {
      phone: '',
      email: '',
    },
    isPartOf: '',
    hasPart: '',
    homepage: '',
    type: '',
    sector: '',
  };

  const handleIdentifierChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.identifier = event.currentTarget.value;
  };

  const handleDescrptionChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.description.nb = event.currentTarget.value;
  };

  const handleTitleChangeChange = (
    event: React.FormEvent<HTMLInputElement>,
  ) => {
    resourceFormResult.title.nb = event.currentTarget.value;
  };

  const handleOrganizationNumberChange = (
    event: React.FormEvent<HTMLInputElement>,
  ) => {
    resourceFormResult.hasCompetentAuthority.organization =
      event.currentTarget.value;
  };

  const handleOrgCodeChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.hasCompetentAuthority.orgCode =
      event.currentTarget.value;
  };

  const handlePhoneChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.contactPoint.phone = event.currentTarget.value;
  };

  const handleEmailChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.contactPoint.email = event.currentTarget.value;
  };

  const handleIsPartOfChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.isPartOf = event.currentTarget.value;
  };

  const handleHasPartChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.hasPart = event.currentTarget.value;
  };

  const handleHomepageChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.homepage = event.currentTarget.value;
  };

  const handleTypeChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.type = event.currentTarget.value;
  };

  const handleSectorChange = (event: React.FormEvent<HTMLInputElement>) => {
    resourceFormResult.sector = event.currentTarget.value;
  };

  const submit = () => {
    console.log(resourceFormResult);
  };

  return (
    <div className={classes['resource-registry-container']}>
      <div className={classes['resource-registry__form']}>
        <div className={classes['button-container']}>
          <h2 className={classes['center']}>Ressursregistrering</h2>
        </div>
        <form>
          <div className={classes['float-container']}>
            <label>Identifikator</label>
            <TextField onChange={(e) => handleIdentifierChange(e)}></TextField>
            <label>Tittel</label>
            <TextField onChange={(e) => handleTitleChangeChange(e)}></TextField>
          </div>
          <div className={classes['float-container']}>
            <label>Organisasjonsnummer</label>
            <TextField
              onChange={(e) => handleOrganizationNumberChange(e)}
            ></TextField>
            <label>Organisasjonskode</label>
            <TextField onChange={(e) => handleOrgCodeChange(e)}></TextField>
          </div>
          <div className={classes['float-container']}>
            <label>Telefonnummer</label>
            <TextField
              name={'phone'}
              onChange={(e) => handlePhoneChange(e)}
            ></TextField>
            <label>Epost</label>
            <TextField
              name={'email'}
              onChange={(e) => handleEmailChange(e)}
            ></TextField>
          </div>
          <div className={classes['float-container']}>
            <label>Beskrivelse</label>
            <TextField
              name={'description'}
              onChange={(e) => handleDescrptionChange(e)}
            ></TextField>
            <label>Hjemmeside</label>
            <TextField onChange={(e) => handleHomepageChange(e)}></TextField>
          </div>
          <div className={classes['float-container']}>
            <label>Er del av</label>
            <TextField onChange={(e) => handleIsPartOfChange(e)}></TextField>
            <label>Har part</label>
            <TextField onChange={(e) => handleHasPartChange(e)}></TextField>
          </div>
          <div className={classes['float-container']}>
            <label>Type</label>
            <TextField onChange={(e) => handleTypeChange(e)}></TextField>
            <label>Sektor</label>
            <TextField onChange={(e) => handleSectorChange(e)}></TextField>
          </div>
        </form>
        <div className={classes['button-container']}>
          <div className={classes['center']}>
            <Button type="submit" onClick={submit}>
              Registrer
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ResourceRegistry;
