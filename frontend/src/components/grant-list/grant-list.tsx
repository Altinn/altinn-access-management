import { Grant } from './models';

export interface GrantListProps {
  grants?: Grant[];
}

const GrantList = ({ grants }: GrantListProps) => {
  if (!grants || grants.length === 0) {
    return <>Nothing to see here…</>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Header 1</th>
        </tr>
      </thead>
      <tbody>
        {grants.map((grant) => (
          <tr key={grant.id}>
            <td>{grant.entity.name}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default GrantList;
